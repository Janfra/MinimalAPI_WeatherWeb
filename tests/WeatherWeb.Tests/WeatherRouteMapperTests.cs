namespace WeatherWeb.Tests;

using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using MockQueryable.Moq;
using Moq;
using WeatherWeb.Data;
using WeatherWeb.EndpointMappers;
using WeatherWeb.Models;
using WeatherWeb.Services.WeatherReport;
using WeatherWeb.Tests.Extensions;

public class WeatherRouteMapperTests
{
    [Fact]
    public async Task GetHotReportsAsync_ShouldReturnOkResult()
    {
        // Arrange
        var hotThreshold = 0;
        var testData = new List<WeatherReportResponse>
        {
            new(1, TemperatureC: 60, Humidity: 50.0f, Location: "CityA"),
            new(2, TemperatureC: -100, Humidity: 50.0f, Location: "CityB"),
            new(3, TemperatureC: hotThreshold, Humidity: 50.0f, Location: "CityC"),
            new(4, TemperatureC: hotThreshold + 1, Humidity: 50.0f, Location: "CityC"),
        };
        var mockReportService = new Mock<IWeatherReportService>();
        mockReportService.Setup(r => r.GetHotReportsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(testData.Where(r => r.TemperatureC > hotThreshold).ToList());

        // Act
        var result = await WeatherRouteMapper.GetHotReportsAsync(mockReportService.Object);

        // Assert
        Assert.NotNull(result);
        var reports = AssertResultValue(result);
        Assert.Equal(2, reports.Count);
        Assert.All(reports, r => Assert.True(r.TemperatureC > hotThreshold));
    }

    [Fact]
    public async Task GetFormattedReportsAsync_ShouldReturnOkResult()
    {
        // Arrange
        string MockFormat(WeatherReport r) => $"Location: {r.Location}, TempC: {r.TemperatureC}, Humidity: {r.Humidity}";
        var testData = new List<WeatherReport>
        {
            new(temperatureC: 60, humidity: 50.0f, location: "CityA") { Id = 1 },
            new(temperatureC: -100, humidity: 30.0f, location: "CityB") { Id = 2 },
        };
        var mockReportService = new Mock<IWeatherReportService>();
        mockReportService.Setup(r => r.GetFormattedReportsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(testData.Select(MockFormat).ToList());

        // Act
        var result = await WeatherRouteMapper.GetFormattedReportsAsync(mockReportService.Object);

        // Assert
        Assert.NotNull(result);
        var reports = AssertResultValue(result);
        Assert.Equal(testData.Count, reports.Count);
        for (int i = 0; i < testData.Count; i++)
        {
            Assert.Contains(MockFormat(testData[i]), reports); // Go through for loop instead of assert equal since data needs to be formatted to match
        }
    }

    [Fact]
    public async Task GetFormattedReportsAsync_ShouldReturnEmptyList_WhenNoReports()
    {
        // Arrange
        var testData = new List<WeatherReport>();
        var mockReportService = new Mock<IWeatherReportService>();
        mockReportService.Setup(r => r.GetFormattedReportsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(testData.Select(r => r.ToString()).ToList());

        // Act
        var result = await WeatherRouteMapper.GetFormattedReportsAsync(mockReportService.Object);

        // Assert
        Assert.NotNull(result);
        var resultValue = AssertResultValue(result);
        Assert.Empty(resultValue);
    }

    [Fact]
    public async Task PostReportDTOAsync_ShouldReturnValidationProblem()
    {
        // Arrange
        var invalidReportDTO = new WeatherReportRequest(
            TemperatureC: -300, // Invalid temperature
            Humidity: 50.0f,
            Location: "CityA"
        );
        var errorKey = "TemperatureC";
        var errorMessage = "TemperatureC must be between -100 and 60.";
        var mockValidator = new Mock<IValidator<WeatherReportRequest>>();
        mockValidator.Setup(v => v.ValidateAsync(invalidReportDTO, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(
            [
                new FluentValidation.Results.ValidationFailure(errorKey, errorMessage)
            ]));
        var mockFilterContext = new Mock<EndpointFilterInvocationContext>().AddArgumentToMock(invalidReportDTO).AddServiceToMock(mockValidator.Object);
        var mockNext = new Mock<EndpointFilterDelegate>();

        // Act
        var result = await WeatherRouteMapper.ValidateReportDTO(mockFilterContext.Object, mockNext.Object);

        // Assert
        var validationProblem = Assert.IsType<ValidationProblem>(result);
        var validationProblemDetails = Assert.IsType<HttpValidationProblemDetails>(validationProblem.ProblemDetails);
        Assert.True(validationProblemDetails.Errors.ContainsKey(errorKey));
        Assert.Contains(errorMessage, validationProblemDetails.Errors[errorKey]);
    }

    [Fact]
    public async Task PostReportDTOAsync_ShouldReturnAddedReport()
    {
        // Arrange
        var testData = new WeatherReportRequest(
            TemperatureC: 30,
            Humidity: 50.0f,
            Location: "CityA"
            );
        WeatherReportResponse testResultData = new(1, testData.TemperatureC, testData.Humidity, testData.Location);
        var mockReportService = new Mock<IWeatherReportService>();
        mockReportService.Setup(m => m.AddWeatherReportAsync(testData, It.IsAny<CancellationToken>())).ReturnsAsync(testResultData);

        // Act
        var result = await WeatherRouteMapper.PostReportDTOAsync(testData, mockReportService.Object);

        // Assert
        Assert.NotNull(result);
        var response = AssertResultValue(result);
        Assert.Equal(response, testResultData);
        Assert.Contains($"/weather/reports/{response.Id}", result.Location);
    }

    [Fact]
    public async Task GetFilteredReportAsync_ShouldGetOkResultWithReport()
    {
        // Arrange
        var minHumidity = 30.0f;
        var testData = new List<WeatherReportResponse>
        {
            new(1, TemperatureC: 10, Humidity: minHumidity, Location: "CityA"),
            new(2, TemperatureC: 20, Humidity: minHumidity + 0.1f, Location: "CityB"),
        };
        var mockReportService = new Mock<IWeatherReportService>();
        mockReportService.Setup(r => r.GetMinHumidityReportsAsync(minHumidity, It.IsAny<CancellationToken>())).ReturnsAsync(testData);

        // Act
        var result = await WeatherRouteMapper.GetFilteredReportAsync(minHumidity, mockReportService.Object);

        // Assert
        Assert.NotNull(result);
        var resultValue = AssertResultValue(result);
        Assert.Equal(resultValue, testData);
    }

    [Fact]
    public async Task GetLocationReportsAsync_ShouldReturnOkResultWithLocations()
    {
        // Arrange
        var Location = "CityA";
        var testData = new List<WeatherReportResponse>
        {
            new(1, TemperatureC: 10, Humidity: 10.0f, Location: Location),
            new(2, TemperatureC: 20, Humidity: 30.0f, Location: Location),
        };

        var mockReportService = new Mock<IWeatherReportService>();
        mockReportService.Setup(r => r.GetLocationReportsAsync(Location, It.IsAny<CancellationToken>())).ReturnsAsync(testData);

        // Act
        var results = await WeatherRouteMapper.GetLocationReportsAsync(Location, mockReportService.Object);

        // Assert
        var okResult = Assert.IsType<Ok<IReadOnlyList<WeatherReportResponse>>>(results.Result);
        var resultValue = AssertResultValue(okResult);
        Assert.Equal(resultValue, testData);
    }

    private static TValue AssertResultValue<TValue>(IValueHttpResult<TValue> result)
    {
        Assert.NotNull(result.Value);
        return result.Value;
    }
}