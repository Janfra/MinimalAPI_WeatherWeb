namespace WeatherWeb.Models;

using System.Text.Json.Serialization;

public record WeatherReportDTO(
    [property: JsonPropertyName("temp_c")] int TemperatureC,
    [property: JsonPropertyName("humidity_percent")] float Humidity,
    [property: JsonPropertyName("location")] string Location)
{
    [JsonPropertyName("temp_f")]
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
