using Serilog;

namespace API.Middleware;

public class LogRequestMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        Log.Information($"request received at {DateTime.UtcNow}");
        await next(context);
        Log.Information($"request processing completed at {DateTime.UtcNow}");
    }
}