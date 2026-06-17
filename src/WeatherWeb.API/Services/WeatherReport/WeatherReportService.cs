namespace WeatherWeb.Services.WeatherReport;

using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using WeatherWeb.Data;
using WeatherWeb.Models;
using WeatherWeb.Services.WeatherFormatter;

public class WeatherReportService(IWeatherFormatter formatter, IWeatherDbContext weatherDb) : IWeatherReportService
{
    public const int HOT_THRESHOLD_DEGREES = 30;
    private readonly IWeatherFormatter _formatter = formatter;
    private readonly IWeatherDbContext _weatherDb = weatherDb;

    public async Task<WeatherReportResponse> AddWeatherReportAsync(WeatherReportRequest weatherReportRequest, CancellationToken cancellationToken = default)
    {
        var entityAdded = _weatherDb.Add(weatherReportRequest.ToEntity());
        await _weatherDb.SaveChangesAsync(cancellationToken);
        return entityAdded.ToResponse();
    }

    public async Task<WeatherReportResponse?> PutWeatherReportAsync(int id, WeatherReportRequest weatherReportRequest, CancellationToken cancellationToken = default)
    {
        var report = await _weatherDb.WeatherReports.FindAsync(id, cancellationToken);
        if (report == null)
        {
            return null;
        }

        report.TemperatureC = weatherReportRequest.TemperatureC;
        report.Humidity = weatherReportRequest.Humidity;
        report.Location = weatherReportRequest.Location;
        await _weatherDb.SaveChangesAsync(cancellationToken);
        return report.ToResponse();
    }

    public async Task<WeatherReportResponse?> DeleteWeatherReportAsync(int id, CancellationToken cancellationToken = default)
    {
        var report = await _weatherDb.WeatherReports.FindAsync(id, cancellationToken);
        if (report == null)
        {
            return null;
        }

        _weatherDb.Remove(report);
        await _weatherDb.SaveChangesAsync(cancellationToken);
        return report.ToResponse(); 
    }

    public async Task<IReadOnlyList<string>> GetFormattedReportsAsync(CancellationToken cancellationToken = default)
    {
        return await _weatherDb.WeatherReports.Select(report => _formatter.Format(report)).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WeatherReportResponse>> GetHotReportsAsync(CancellationToken cancellationToken = default)
    {
        return await _weatherDb.WeatherReports.Where(report => report.TemperatureC >= HOT_THRESHOLD_DEGREES).Select(report => report.ToResponse()).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WeatherReportResponse>> GetLocationReportsAsync(string location, CancellationToken cancellationToken = default)
    {
        return await _weatherDb.WeatherReports.Where(report => report.Location.Equals(location)).Select(report => report.ToResponse()).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WeatherReportResponse>> GetMinHumidityReportsAsync(float humidityThreshold, CancellationToken cancellationToken = default)
    {
        return await _weatherDb.WeatherReports.Where(report => report.Humidity >= humidityThreshold).Select(report => report.ToResponse()).ToListAsync(cancellationToken);
    }
}