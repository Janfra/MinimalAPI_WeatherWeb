namespace WeatherWeb.Models;

using System.Text.Json.Serialization;

public record WeatherReportRequest(
    [property: JsonPropertyName("temp_c")] int TemperatureC,
    [property: JsonPropertyName("humidity_percent")] float Humidity,
    [property: JsonPropertyName("location")] string Location)
{
    public WeatherReportRequest ToEntity() => new WeatherReportRequest(TemperatureC, Humidity, Location);
}
