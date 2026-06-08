namespace WeatherWeb.Tests;

using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using MockQueryable.Moq;
using Moq;
using WeatherWeb.Data;
using WeatherWeb.EndpointMappers;
using WeatherWeb.Models;
using WeatherWeb.Services.Reporter;
using WeatherWeb.Tests.Extensions;

public class WeatherRouteMapperTests
{
    [Fact]
    public async Task GetHotReportsAsync_ShouldReturnOkResult()
    {
        // Arrange
        var hotThreshold = 0;
        var testData = new List<WeatherReport>
        {
            new(temperatureC: 60, humidity: 50.0f, location: "CityA") { Id = 1 },
            new(temperatureC: -100, humidity: 50.0f, location: "CityB") { Id = 2 },
            new(temperatureC: 0, humidity: 50.0f, location: "CityC") { Id = 3 },
            new(temperatureC: 1, humidity: 50.0f, location: "CityC") { Id = 4 },
        };
        var mockDb = CreateMockFromTestData(testData);
        var mockReporter = new Mock<IWeatherReporter>();
        mockReporter.Setup(r => r.GetHotReportsAsync(It.IsAny<IQueryable<WeatherReport>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(testData.Where(r => r.TemperatureC > hotThreshold).ToList());

        // Act
        var result = await WeatherRouteMapper.GetHotReportsAsync(mockReporter.Object, mockDb.Object);

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
        var mockDb = CreateMockFromTestData(testData);
        var mockReporter = new Mock<IWeatherReporter>();
        mockReporter.Setup(r => r.GetFormattedReportsAsync(It.IsAny<IQueryable<WeatherReport>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(testData.Select(MockFormat).ToList());

        // Act
        var result = await WeatherRouteMapper.GetFormattedReportsAsync(mockReporter.Object, mockDb.Object);

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
        var mockDb = CreateMockFromTestData(testData);
        var mockReporter = new Mock<IWeatherReporter>();
        mockReporter.Setup(r => r.GetFormattedReportsAsync(It.IsAny<IQueryable<WeatherReport>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(testData.Select(r => r.ToString()).ToList());

        // Act
        var result = await WeatherRouteMapper.GetFormattedReportsAsync(mockReporter.Object, mockDb.Object);

        // Assert
        Assert.NotNull(result);
        var resultValue = AssertResultValue(result);
        Assert.Empty(resultValue);
    }

    [Fact]
    public async Task PostReportDTOAsync_ShouldReturnValidationProblem()
    {
        // Arrange
        var invalidReportDTO = new WeatherReportDTO(
            TemperatureC: -300, // Invalid temperature
            Humidity: 50.0f,
            Location: "CityA"
        );
        var errorKey = "TemperatureC";
        var errorMessage = "TemperatureC must be between -100 and 60.";
        var mockValidator = new Mock<IValidator<WeatherReportDTO>>();
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
        var testData = new WeatherReportDTO(
            TemperatureC: 30,
            Humidity: 50.0f,
            Location: "CityA"
            );
        WeatherReport testResultData = new(testData.TemperatureC, testData.Humidity, testData.Location) { Id = 1 };
        var mockDb = CreateMockFromTestData(new());
        mockDb.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        mockDb.Setup(db => db.Add<WeatherReport>(It.IsAny<WeatherReport>())).Returns(testResultData);

        // Act
        var result = await WeatherRouteMapper.PostReportDTOAsync(testData, mockDb.Object);

        // Assert
        Assert.NotNull(result);
        var createdEntity = AssertResultValue(result);
        Assert.Equal(createdEntity, testResultData);
        Assert.Contains($"/weather/reports/{createdEntity.Id}", result.Location);
    }

    [Fact]
    public async Task GetFilteredReportAsync_ShouldGetOkResultWithReport()
    {
        // Arrange
        var minHumidity = 30.0f;
        var testData = new List<WeatherReport>
        {
            new(temperatureC: 10, humidity: minHumidity, location: "CityA") { Id = 1 },
            new(temperatureC: 20, humidity: minHumidity + 0.1f, location: "CityB") { Id = 2 },
        };
        var mockDb = CreateMockFromTestData(testData);
        var mockReporter = new Mock<IWeatherReporter>();
        mockReporter.Setup(r => r.GetMinHumidityReportsAsync(It.IsAny<IQueryable<WeatherReport>>(), minHumidity, It.IsAny<CancellationToken>())).ReturnsAsync(testData);

        // Act
        var result = await WeatherRouteMapper.GetFilteredReportAsync(minHumidity, mockReporter.Object, mockDb.Object);

        // Assert
        Assert.NotNull(result);
        var resultValue = AssertResultValue(result);
        Assert.Equal(resultValue, testData);
    }

    [Fact]
    public async Task GetLocationReportsAsync_ShouldReturnOkResultWithLocations()
    {
        // Arrange
        var location = "CityA";
        var testData = new List<WeatherReport>
        {
            new(temperatureC: 10, humidity: 10.0f, location: location) { Id = 1 },
            new(temperatureC: 20, humidity: 30.0f, location: location) { Id = 2 },
        };

        var mockDb = CreateMockFromTestData(testData);
        var mockReporter = new Mock<IWeatherReporter>();
        mockReporter.Setup(r => r.GetLocationReportsAsync(It.IsAny<IQueryable<WeatherReport>>(), location, It.IsAny<CancellationToken>())).ReturnsAsync(testData);

        // Act
        var results = await WeatherRouteMapper.GetLocationReportsAsync(location, mockReporter.Object, mockDb.Object);

        // Assert
        var okResult = Assert.IsType<Ok<IReadOnlyList<WeatherReport>>>(results.Result);
        var resultValue = AssertResultValue(okResult);
        Assert.Equal(resultValue, testData);
    }

    private static TValue AssertResultValue<TValue>(IValueHttpResult<TValue> result)
    {
        Assert.NotNull(result.Value);
        return result.Value;
    }

    private static Mock<IWeatherDbContext> CreateMockFromTestData(List<WeatherReport> testData)
    {
        var mockSet = testData.BuildMockDbSet();
        var mockDb = new Mock<IWeatherDbContext>();
        mockDb.Setup(db => db.WeatherReports).Returns(mockSet.Object);
        return mockDb;
    }
}