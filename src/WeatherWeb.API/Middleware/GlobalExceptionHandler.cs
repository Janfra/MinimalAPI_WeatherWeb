namespace WeatherWeb.Middleware;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment environment) : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;
    private readonly IHostEnvironment _environment = environment;
    public const string DefaultErrorDetailInProd = "An internal error occurred. Please contact support if the issue persists.";
    public const string DetailsContentType = "application/problem+json";
    public const string ProblemDetailsDefaultTitle = "An unexpected Server Error occurred.";

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An unhandled exception occurred while processing the request. {Message}", exception.Message);

        // In production replace message to avoid leaking sensitive information and provide more relevant information to the user.
        var detail = _environment.IsDevelopment() ? exception.ToString() : DefaultErrorDetailInProd; 
        var problemDetails = new ProblemDetails
        {
            Title = ProblemDetailsDefaultTitle,
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
            contentType: DetailsContentType,
            cancellationToken);

        return true;
    }
}

