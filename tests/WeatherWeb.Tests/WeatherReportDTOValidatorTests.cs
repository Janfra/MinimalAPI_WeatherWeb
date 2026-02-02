namespace WeatherWeb.Tests;

using WeatherWeb.Models;
using WeatherWeb.Validators;
using Xunit;

public class WeatherReportDTOValidatorTests
{
    [Theory]
    [InlineData(25, 50.0f, "City1", true)]
    [InlineData(-100, 50.0f, "City2", true)] // Valid: Temperature at lower boundary
    [InlineData(60, 50.0f, "City3", true)] // Valid: Temperature at higher boundary
    [InlineData(30, 0.0f, "City4", true)] // Valid: Humidity at lower boundary
    [InlineData(30, 100.0f, "City5", true)] // Valid: Humidity at higher boundary
    [InlineData(-101, 50.0f, "City6", false)] // Invalid: Temperature below limit
    [InlineData(61, 50.0f, "City7", false)] // Invalid: Temperature over limit  
    [InlineData(30, -0.1f, "City8", false)] // Invalid: Humidity below limit
    [InlineData(20, 100.1f, "City9", false)] // Invalid: Humidity over limit
    [InlineData(15, 50.0f, "City10 AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", false)] // Invalid: Location too long
    [InlineData(15, 50.0f, "City11_", false)] // Invalid: Underscore in location
    [InlineData(15, 50.0f, "City12  A", false)] // Invalid: Consecutive spaces
    [InlineData(15, 50.0f, "", false)] // Invalid: Empty location
    public async Task Validate_ShouldReturnExpectedResults(int temperatureC, float humidity, string location, bool isValidExpected)
    {
        // Arrange
        var validator = new WeatherReportDTOValidator();
        var dto = new WeatherReportDTO(temperatureC, humidity, location);

        // Act
        var validationResult = await validator.ValidateAsync(dto);

        // Assert
        Assert.Equal(isValidExpected, validationResult.IsValid);
    }
}