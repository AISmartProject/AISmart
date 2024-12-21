using System.Diagnostics;
using AISmart.Basic;

namespace AISmart.AuthServer.Middleware;

public class TimeTrackingStatisticsAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TimeTrackingStatisticsAuthMiddleware> _logger;

    public TimeTrackingStatisticsAuthMiddleware(RequestDelegate next, ILogger<TimeTrackingStatisticsAuthMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

            if (context.Response.StatusCode == CommonConstant.HttpSuccessCode || context.Response.StatusCode == CommonConstant.HttpFileUploadSuccessCode)
            {
                _logger.LogInformation(
                    "TimeTrackingStatisticsAuthMiddleware Path {path} Method {Method} StatusCode {StatusCode} Request took {elapsedMilliseconds} ms ",
                    context.Request.Path, context.Request.Method, context.Response.StatusCode, elapsedMilliseconds);
            }
            else
            {
                _logger.LogInformation(
                    "TimeTrackingStatisticsAuthMiddleware Path {path} Method {Method} StatusCode {StatusCode} Authorization {Authorization} Request took {elapsedMilliseconds} ms ",
                    context.Request.Path, context.Request.Method, context.Response.StatusCode,
                    context.Request.Headers.Authorization, elapsedMilliseconds);
            }
        }
    }
}
