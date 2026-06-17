namespace WeatherWeb.Models;

using System.Text.Json.Serialization;

public record WeatherReportResponse(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("temp_c")] int TemperatureC,
    [property: JsonPropertyName("humidity_percent")] float Humidity,
    [property: JsonPropertyName("location")] string Location)
{
    [JsonPropertyName("temp_f")]
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    public static WeatherReportResponse FromEntity(WeatherReport entity) => new(entity.Id, entity.TemperatureC, entity.Humidity, entity.Location);
}