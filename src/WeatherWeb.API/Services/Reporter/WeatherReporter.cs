namespace WeatherWeb.Services.Reporter;

using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using WeatherWeb.Models;
using WeatherWeb.Services.Formatter;

public class WeatherReporter(IWeatherFormatter formatter) : IWeatherReporter
{
    public const int HOT_THRESHOLD_DEGREES = 30;
    private readonly IWeatherFormatter _formatter = formatter;

    public async Task<IReadOnlyList<string>> GetFormattedReportsAsync(IQueryable<WeatherReport> reports, CancellationToken cancellationToken = default)
    {
        return await reports.Select(report => _formatter.Format(report)).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WeatherReport>> GetHotReportsAsync(IQueryable<WeatherReport> reports, CancellationToken cancellationToken = default)
    {
        return await reports.Where(report => report.TemperatureC >= HOT_THRESHOLD_DEGREES).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WeatherReport>> GetLocationReportsAsync(IQueryable<WeatherReport> reports, string location, CancellationToken cancellationToken = default)
    {
        return await reports.Where(report => report.Location.Equals(location)).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WeatherReport>> GetMinHumidityReportsAsync(IQueryable<WeatherReport> reports, float humidityThreshold, CancellationToken cancellationToken = default)
    {
        return await reports.Where(report => report.Humidity >= humidityThreshold).ToListAsync(cancellationToken);
    }
}