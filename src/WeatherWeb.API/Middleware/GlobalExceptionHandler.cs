namespace WeatherWeb.Middleware;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment environment) : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;
    private readonly IHostEnvironment _environment = environment;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An unhandled exception occurred while processing the request. {Message}", exception.Message);

        // In production replace message to avoid leaking sensitive information and provide more relevant information to the user.
        var detail = _environment.IsDevelopment() ? exception.ToString() : "An internal error ocurred. Please support if the issue persists."; 
        var problemDetails = new ProblemDetails
        {
            Title = "An unexpected Server Error occurred.",
            Detail = detail,
            Status = StatusCodes.Status500InternalServerError,
            Instance = httpContext.Request.Path,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;
        // Ensure the content type is as expected
        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            options: null,
            contentType: "application/problem+json",
            cancellationToken);

        return true;
    }
}

