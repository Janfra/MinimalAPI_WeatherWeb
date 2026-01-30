namespace WeatherWeb.Services.Reporter;

using WeatherWeb.Models;

public interface IWeatherReporter
{
    public Task<IReadOnlyList<WeatherReport>> GetHotReportsAsync(IQueryable<WeatherReport> reports);
    public Task<IReadOnlyList<WeatherReport>> GetLocationReportsAsync(IQueryable<WeatherReport> reports, string location);
    public Task<IReadOnlyList<string>> GetFormattedReportsAsync(IQueryable<WeatherReport> reports);
    public Task<IReadOnlyList<WeatherReport>> GetMinHumidityReportsAsync(IQueryable<WeatherReport> reports, float humidityThreshold);
}
