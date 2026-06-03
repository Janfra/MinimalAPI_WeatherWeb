namespace WeatherWeb.Tests;

using Moq;
using MockQueryable.Moq;
using WeatherWeb.Models;
using WeatherWeb.Services.Formatter;
using WeatherWeb.Services.Reporter;

public class WeatherReporterTests
{
    [Fact]
    public async Task GetHotReportsAsync_ShouldOnlyReturnReportsOverOrEqualHotThresholdDegrees()
    {
        // Arrange
        var mockFormatter = new Mock<IWeatherFormatter>();
        var weatherReporter = new WeatherReporter(mockFormatter.Object);

        var testData = new List<WeatherReport> {
            new(temperatureC: WeatherReporter.HOT_THRESHOLD_DEGREES, humidity: 50, location: "CityA"),
            new(temperatureC: WeatherReporter.HOT_THRESHOLD_DEGREES - 1, humidity: 80, location: "CityB"),
            new(temperatureC: WeatherReporter.HOT_THRESHOLD_DEGREES + 1, humidity: 80, location: "CityC"),
            new(temperatureC: 60, humidity: 65, location: "CityD")
        };

        var mockDbSet = testData.BuildMockDbSet().Object;

        // Act
        var resultList = await weatherReporter.GetHotReportsAsync(mockDbSet);

        // Assert
        // Since 60 is the limit of heat, we expect 3 results: the threshold, the slightly over the threshold, and 60
        Assert.Equal(3, resultList.Count);
        Assert.All(resultList, report => {             
            Assert.True(report.TemperatureC >= WeatherReporter.HOT_THRESHOLD_DEGREES);
        });
    }

    [Fact]
    public async Task GetLocationReportsAsync_ShouldReturnOnlyReportsMatchingLocation()
    {
        // Arrange
        var mockFormatter = new Mock<IWeatherFormatter>();
        var weatherReporter = new WeatherReporter(mockFormatter.Object);
        var targetLocation = "CityA";
        var testData = new List<WeatherReport> {
            new(temperatureC: 25, humidity: 50, location: targetLocation),
            new(temperatureC: 30, humidity: 80, location: "CityB"),
            new(temperatureC: 20, humidity: 80, location: targetLocation),
            new(temperatureC: 15, humidity: 65, location: "CityC"),
            new(temperatureC: 10, humidity: 65, location: "CityB")
        };
        var mockDbSet = testData.BuildMockDbSet().Object;

        // Act
        var resultList = await weatherReporter.GetLocationReportsAsync(mockDbSet, targetLocation);

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
        var weatherReporter = new WeatherReporter(mockFormatter.Object);
        var humidityThreshold = 70.0f;
        var testData = new List<WeatherReport> {
            new(temperatureC: 25, humidity: humidityThreshold, location: "CityA"), // Meet
            new(temperatureC: 30, humidity: humidityThreshold + 1, location: "CityB"), // Meet
            new(temperatureC: 20, humidity: humidityThreshold - 1, location: "CityC"), // Below
        };
        var mockDbSet = testData.BuildMockDbSet().Object;

        // Act
        var resultList = await weatherReporter.GetMinHumidityReportsAsync(mockDbSet, humidityThreshold);

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
        string MockFormat(WeatherReport report)
        {
            return $"Formatted: {report.TemperatureC}C, {report.Humidity}%, {report.Location}";
        }
            
        var mockFormatter = new Mock<IWeatherFormatter>();
        mockFormatter.Setup(f => f.Format(It.IsAny<WeatherReport>()))
            .Returns<WeatherReport>(MockFormat);
        var weatherReporter = new WeatherReporter(mockFormatter.Object);
        var testData = new List<WeatherReport> {
            new(temperatureC: 25, humidity: 50, location: "CityA"),
            new(temperatureC: 30, humidity: 80, location: "CityB"),
            new(temperatureC: 20, humidity: 80, location: "CityC"),
        };
        var mockDbSet = testData.BuildMockDbSet().Object;

        // Act
        var resultList = await weatherReporter.GetFormattedReportsAsync(mockDbSet);

        // Assert
        Assert.Equal(testData.Count, resultList.Count);
        for (int i = 0; i < testData.Count; i++)
        {
            var expectedString = MockFormat(testData[i]);
            Assert.Equal(expectedString, resultList[i]);
        }
    }
}
