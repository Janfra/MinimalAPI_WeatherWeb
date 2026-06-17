namespace WeatherWeb.Services.WeatherReport;

using WeatherWeb.Models;

public interface IWeatherReportService
{
    public Task<WeatherReportResponse> AddWeatherReportAsync(WeatherReportRequest weatherReportRequest, CancellationToken cancellationToken = default);
    public Task<WeatherReportResponse?> PutWeatherReportAsync(int id, WeatherReportRequest weatherReportRequest, CancellationToken cancellationToken = default);
    public Task<WeatherReportResponse?> DeleteWeatherReportAsync(int id, CancellationToken cancellationToken = default);
    public Task<IReadOnlyList<WeatherReportResponse>> GetHotReportsAsync(CancellationToken cancellationToken = default);
    public Task<IReadOnlyList<WeatherReportResponse>> GetLocationReportsAsync(string location, CancellationToken cancellationToken = default);
    public Task<IReadOnlyList<string>> GetFormattedReportsAsync(CancellationToken cancellationToken = default);
    public Task<IReadOnlyList<WeatherReportResponse>> GetMinHumidityReportsAsync(float humidityThreshold, CancellationToken cancellationToken = default);
}
