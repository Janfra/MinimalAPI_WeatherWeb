namespace WeatherWeb.Tests;

using System;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using WeatherWeb.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

public class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_ShouldReturn500ProblemDetails()
    {
        var exceptionHandler = GetNewGlobalExceptionHandlerToTest(isDevelopment: true);
        var context = GetBlankHttpContextWithMemoryStream();
        var exception = new Exception("Critical Failure");

        var result = await exceptionHandler.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.True(result); // Error must have been handled.
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.StartsWith(GlobalExceptionHandler.DetailsContentType, context.Response.ContentType);
        // Retrieve the information from the modified context to test for correct contents.
        var problemDetails = await TryGetProblemDetailsFromContext(context);
        Assert.NotNull(problemDetails);
        Assert.Equal(GlobalExceptionHandler.ProblemDetailsDefaultTitle, problemDetails.Title);
        Assert.Equal(StatusCodes.Status500InternalServerError, problemDetails.Status);
    }

    [Fact]
    public async Task TryHandleAsync_ShouldProvideDefaultErrorMessageInDetailsWhenInProd()
    {
        var exceptionHandler = GetNewGlobalExceptionHandlerToTest(isDevelopment: false);
        var context = GetBlankHttpContextWithMemoryStream();
        var exception = new Exception("Critical Failure");

        var result = await exceptionHandler.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.True(result);
        var problemDetails = await TryGetProblemDetailsFromContext(context);
        Assert.NotNull(problemDetails);
        Assert.Equal(GlobalExceptionHandler.DefaultErrorDetailInProd, problemDetails.Detail);
    }

    [Fact]
    public async Task TryHandleAsync_ShouldIncludeProblemDetailsWithFullExceptionWhenInDevelopment()
    {
        var exceptionHandler = GetNewGlobalExceptionHandlerToTest(isDevelopment: true);
        var context = GetBlankHttpContextWithMemoryStream();
        var exception = new Exception("Critical Failure");

        var result = await exceptionHandler.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.True(result);
        var problemDetails = await TryGetProblemDetailsFromContext(context);
        Assert.NotNull(problemDetails);
        Assert.Equal(exception.ToString(), problemDetails.Detail);  
    }

    private HttpContext GetBlankHttpContextWithMemoryStream()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
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

    private GlobalExceptionHandler GetNewGlobalExceptionHandlerToTest(bool isDevelopment)
    {
        var loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
        var environmentMock = GetMockHostEnvironment(isDevelopment);
        return new GlobalExceptionHandler(loggerMock.Object, environmentMock.Object);
    }

    private Mock<IHostEnvironment> GetMockHostEnvironment(bool isDevelopment)
    {
        var environmentMock = new Mock<IHostEnvironment>();
        string environmentName = isDevelopment ? Environments.Development : Environments.Production;
        environmentMock.Setup(env => env.EnvironmentName).Returns(environmentName);
        return environmentMock;
    }
}
