namespace WeatherWeb.Models;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class WeatherReport(
    int TemperatureC,
    float Humidity,
    string Location)
{
    [Key]
    public int Id { get; init; }

    [JsonPropertyName("temp_c")]
    public int TemperatureC { get; set; } = TemperatureC;

    [JsonPropertyName("humidity_percent")]
    public float Humidity { get; set; } = Humidity;

    [JsonPropertyName("location")]
    public string Location { get; set; } = Location;

    [JsonPropertyName("temp_f")]
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    public override string ToString()
    {
        return $"Location: {Location} - Temperature: {TemperatureC}°C, Humidity: {Humidity}%";
    }

    public WeatherReportDTO ToDataTransferObject() => new WeatherReportDTO(TemperatureC, Humidity, Location);
}