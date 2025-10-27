using System.Net;
using System.Text.Json;
using API.Errors;
using Serilog;

namespace API.Middleware;

public class ExceptionMiddleware : IMiddleware
{
    private IHostEnvironment environment { get; set; }

    public ExceptionMiddleware(IHostEnvironment env)
    {
        environment = env;
    }
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception e)
        {
            await HandleExceptionAsync(context, e, environment);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception, IHostEnvironment hostEnvironment)
    {
        Log.Error($"exception : {exception.Message}, stackTrace : {exception.StackTrace}");
        
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = environment.IsDevelopment()
            ? new ApiErrorResponse(context.Response.StatusCode, exception.Message, exception.StackTrace)
            : new ApiErrorResponse(context.Response.StatusCode, exception.Message, "Internal Server Error");
        
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        var json = JsonSerializer.Serialize(response, options);

        await context.Response.WriteAsync(json);
    }
}