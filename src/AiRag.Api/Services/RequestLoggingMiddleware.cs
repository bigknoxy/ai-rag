using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AiRag.Api.Services;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        var request = context.Request;
        _logger.LogInformation("Incoming request {method} {path}", request.Method, request.Path);

        // Capture request body if present (non-invasive, only for small bodies)
        string? requestBody = null;
        if (request.ContentLength > 0 && request.ContentLength < 4096 && request.Body.CanSeek)
        {
            try
            {
                request.Body.Position = 0;
                using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
                requestBody = await reader.ReadToEndAsync();
                request.Body.Position = 0;
            }
            catch
            {
                // ignore reading errors
            }
        }

        await _next(context);

        sw.Stop();
        var response = context.Response;
        _logger.LogInformation("Handled {method} {path} => {status} in {ms}ms", request.Method, request.Path, response.StatusCode, sw.ElapsedMilliseconds);
    }
}
