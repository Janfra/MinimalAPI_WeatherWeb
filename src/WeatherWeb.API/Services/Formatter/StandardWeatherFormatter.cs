namespace WeatherWeb.Services.Formatter;

using WeatherWeb.Models; 

public class StandardWeatherFormatter : IWeatherFormatter
{
    public string Format(WeatherReport report)
    {
        return $"[Standard Report] Temperature: {report.TemperatureC}°C - Humidity: {report.Humidity}% - Location: {report.Location}";
    }

    public string GetTemperatureFormat(WeatherReport report) => report switch
    {
        { TemperatureC: < 0 } => "Freezing",
        { TemperatureC: < 15 } => "Cold",
        { TemperatureC: < 22 } => "Mild",
        { TemperatureC: < 30 } => "Warm",
        _ => "Hot"
    };
}