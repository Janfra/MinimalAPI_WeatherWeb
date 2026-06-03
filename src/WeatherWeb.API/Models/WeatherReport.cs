namespace WeatherWeb.Models;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class WeatherReport
{
    [Key]
    public int Id { get; init; }

    [JsonPropertyName("temp_c")]
    public int TemperatureC { get; set; }

    [JsonPropertyName("humidity_percent")]
    public float Humidity { get; set; }

    [JsonPropertyName("location")]
    public string Location { get; set; }

    [JsonPropertyName("temp_f")]
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    protected WeatherReport() 
    {
        Location = string.Empty;
        Humidity = 0;
        TemperatureC = 0;
    }

    public WeatherReport(int temperatureC, float humidity, string location)
    {
        TemperatureC = temperatureC;
        Humidity = humidity;
        Location = location ?? string.Empty;
        Location = Location.Trim();
    }

    public override string ToString()
    {
        return $"Location: {Location} - Temperature: {TemperatureC}°C, Humidity: {Humidity}%";
    }

    public WeatherReportDTO ToDataTransferObject() => new WeatherReportDTO(TemperatureC, Humidity, Location);
}