namespace WeatherWeb.Tests;

using Moq;
using MockQueryable.Moq;
using WeatherWeb.Models;
using WeatherWeb.Data;
using WeatherWeb.Services.WeatherReport;
using WeatherWeb.Services.WeatherFormatter;

public class WeatherReportServiceTests
{
    [Fact]
    public async Task GetHotReportsAsync_ShouldOnlyReturnReportsOverOrEqualHotThresholdDegrees()
    {
        // Arrange
        var testData = new List<WeatherReport> {
            new(temperatureC: WeatherReportService.HOT_THRESHOLD_DEGREES, humidity: 50, location: "CityA"),
            new(temperatureC: WeatherReportService.HOT_THRESHOLD_DEGREES - 1, humidity: 80, location: "CityB"),
            new(temperatureC: WeatherReportService.HOT_THRESHOLD_DEGREES + 1, humidity: 80, location: "CityC"),
            new(temperatureC: 60, humidity: 65, location: "CityD")
        };
        var testDbSet = testData.BuildMockDbSet().Object;
        var mockWeatherDb = new Mock<IWeatherDbContext>();
        mockWeatherDb.Setup(m => m.WeatherReports).Returns(testDbSet);
        var mockFormatter = new Mock<IWeatherFormatter>();
        var weatherReportService = new WeatherReportService(mockFormatter.Object, mockWeatherDb.Object);

        // Act
        var resultList = await weatherReportService.GetHotReportsAsync();

        // Assert
        // Since 60 is the limit of heat, we expect 3 results: the threshold, the slightly over the threshold, and 60
        Assert.Equal(3, resultList.Count);
        Assert.All(resultList, report => {             
            Assert.True(report.TemperatureC >= WeatherReportService.HOT_THRESHOLD_DEGREES);
        });
    }

    [Fact]
    public async Task GetLocationReportsAsync_ShouldReturnOnlyReportsMatchingLocation()
    {
        // Arrange
        var mockFormatter = new Mock<IWeatherFormatter>();
        var targetLocation = "CityA";
        var testData = new List<WeatherReport> {
            new(temperatureC: 25, humidity: 50, location: targetLocation),
            new(temperatureC: 30, humidity: 80, location: "CityB"),
            new(temperatureC: 20, humidity: 80, location: targetLocation),
            new(temperatureC: 15, humidity: 65, location: "CityC"),
            new(temperatureC: 10, humidity: 65, location: "CityB")
        };
        var testDbSet = testData.BuildMockDbSet().Object;
        var mockWeatherDb = new Mock<IWeatherDbContext>();
        mockWeatherDb.Setup(m => m.WeatherReports).Returns(testDbSet);
        var weatherReportService = new WeatherReportService(mockFormatter.Object, mockWeatherDb.Object);

        // Act
        var resultList = await weatherReportService.GetLocationReportsAsync(targetLocation);

        // Assert
        Assert.All(resultList, report => {
            Assert.Equal(targetLocation, report.Location);
        });
        Assert.Equal(2, resultList.Count); // There are two reports for CityA in the test data
    }

    [Fact]
    public async Task GetMinHumidityReportsAsync_ShouldReturnOnlyReportsMeetingOrExceedingHumidityThreshold()
    {
        // Arrange
        var mockFormatter = new Mock<IWeatherFormatter>();
        var humidityThreshold = 70.0f;
        var testData = new List<WeatherReport> {
            new(temperatureC: 25, humidity: humidityThreshold, location: "CityA"), // Meet
            new(temperatureC: 30, humidity: humidityThreshold + 1, location: "CityB"), // Meet
            new(temperatureC: 20, humidity: humidityThreshold - 1, location: "CityC"), // Below
        };
        var testDbSet = testData.BuildMockDbSet().Object;
        var mockWeatherDb = new Mock<IWeatherDbContext>();
        mockWeatherDb.Setup(m => m.WeatherReports).Returns(testDbSet);
        var weatherReporter = new WeatherReportService(mockFormatter.Object, mockWeatherDb.Object);

        // Act
        var resultList = await weatherReporter.GetMinHumidityReportsAsync(humidityThreshold);

        // Assert
        Assert.All(resultList, report => {
            Assert.True(report.Humidity >= humidityThreshold);
        });
        Assert.Equal(2, resultList.Count); // There are two reports meeting or exceeding the humidity threshold
    }

    [Fact]
    public async Task GetFormattedReportsAsync_ShouldReturnFormattedStringsForAllReports()
    {
        // Arrange
        string MockFormat(WeatherReport report) => $"Formatted: {report.TemperatureC}C, {report.Humidity}%, {report.Location}";
        var mockFormatter = new Mock<IWeatherFormatter>();
        mockFormatter.Setup(f => f.Format(It.IsAny<WeatherReport>()))
            .Returns<WeatherReport>(MockFormat);
        var testData = new List<WeatherReport> {
            new(temperatureC: 25, humidity: 50, location: "CityA"),
            new(temperatureC: 30, humidity: 80, location: "CityB"),
            new(temperatureC: 20, humidity: 80, location: "CityC"),
        };
        var testDbSet = testData.BuildMockDbSet().Object;
        var mockWeatherDb = new Mock<IWeatherDbContext>();
        mockWeatherDb.Setup(m => m.WeatherReports).Returns(testDbSet);
        var weatherReporter = new WeatherReportService(mockFormatter.Object, mockWeatherDb.Object);

        // Act
        var resultList = await weatherReporter.GetFormattedReportsAsync();

        // Assert
        Assert.Equal(testData.Count, resultList.Count);
        for (int i = 0; i < testData.Count; i++)
        {
            var expectedString = MockFormat(testData[i]);
            Assert.Equal(expectedString, resultList[i]);
        }
    }
}
