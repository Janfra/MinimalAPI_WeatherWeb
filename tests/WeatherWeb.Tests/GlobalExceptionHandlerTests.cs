namespace WeatherWeb.Tests;

using System;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using WeatherWeb.Middleware;
using Microsoft.AspNetCore.Mvc;

public class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_ShouldReturn500ProblemDetails()
    {
        var loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
        var handler = new GlobalExceptionHandler(loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = new Exception("Critical Failure");

        var result = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.True(result); // Error must have been handled.
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.StartsWith("application/problem+json", context.Response.ContentType);
        // Retrieve the information from the modified context to test for correct contents.
        var problemDetails = await TryGetProblemDetailsFromContext(context);
        Assert.NotNull(problemDetails);
        Assert.Equal("An unexpected Server Error occurred.", problemDetails.Title);
        Assert.Equal(StatusCodes.Status500InternalServerError, problemDetails.Status);
    }

    private async Task<ProblemDetails?> TryGetProblemDetailsFromContext(HttpContext context)
    {
        if (context == null || context.Response == null || context.Response.Body == null)
        {
            return null;
        }

        context.Response.Body.Seek(0, SeekOrigin.Begin); // Set the file cursor position to the beginning of the file to read all of it
        using var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody);
        return problemDetails;
    }
}
