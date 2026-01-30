namespace WeatherWeb.Services.Reporter;

using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using WeatherWeb.Models;
using WeatherWeb.Services.Formatter;

public class WeatherReporter(IWeatherFormatter formatter) : IWeatherReporter
{
    private readonly IWeatherFormatter _formatter = formatter;

    public async Task<IReadOnlyList<string>> GetFormattedReportsAsync(IQueryable<WeatherReport> reports)
    {
        return await reports.Select(report => _formatter.Format(report)).ToListAsync();
    }

    public async Task<IReadOnlyList<WeatherReport>> GetHotReportsAsync(IQueryable<WeatherReport> reports)
    {
        return await reports.Where(report => report.TemperatureC >= 30).ToListAsync();
    }

    public async Task<IReadOnlyList<WeatherReport>> GetLocationReportsAsync(IQueryable<WeatherReport> reports, string location)
    {
        return await reports.Where(report => report.Location.Equals(location)).ToListAsync();
    }

    public async Task<IReadOnlyList<WeatherReport>> GetMinHumidityReportsAsync(IQueryable<WeatherReport> reports, float humidityThreshold)
    {
        return await reports.Where(report => report.Humidity >= humidityThreshold).ToListAsync();
    }
}