using API.Middleware;
using Core.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Context;
using Serilog.Core;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

//configure serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .WriteTo.Console()
    .WriteTo.File("logs/ticketify-.log", rollingInterval: RollingInterval.Day)

    // application insights sink - logs go to Azure
    .WriteTo.ApplicationInsights(
        builder.Configuration["ApplicationInsights:ConnectionString"],
        TelemetryConverter.Traces)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddApplicationInsightsTelemetry();

builder.Services.AddControllers();

builder.Services.AddDbContext<TicketContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
        //.LogTo(Console.WriteLine, new [] { DbLoggerCategory.Database.Command.Name }, LogLevel.Error );
});
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<ExceptionMiddleware>();
builder.Services.AddScoped<LogRequestMiddleware>();

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<LogRequestMiddleware>();
app.UseHttpsRedirection();
app.MapControllers();

try
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<TicketContext>();
    await context.Database.EnsureCreatedAsync();
    await context.Database.MigrateAsync(); //pending migrations.
    await TicketContextSeed.SeedAsync(context);
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
    throw;
}

//port related configurations.
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://0.0.0.0:{port}");

app.Run();