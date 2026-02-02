namespace WeatherWeb.Mappers;

using FluentValidation;
using FluentValidation.Results;
using WeatherWeb.Data;
using WeatherWeb.Models;
using WeatherWeb.Services.Reporter;

public class WeatherMapper : IMapper
{
    public void Map(WebApplication app)
    {
        app.MapGet("/weather/hot", GetHotReportsAsync);

        app.MapGet("/weather/format", GetFormattedReportsAsync);

        app.MapPost("/weather/reports", PostReportDTOAsync);

        app.MapGet("weather/filter", GetFilteredReportAsync);

        app.MapGet("weather/{location}", GetLocationReportsAsync);

        app.MapPut("weather/reports/{id:int}", PutReportDTOAsync);

        app.MapDelete("weather/reports/{id:int}", DeleteReportAsync);
    }

    public async Task<IResult> GetHotReportsAsync(IWeatherReporter reporter, WeatherDbContext database)
    {
        var hotReportsTask = reporter.GetHotReportsAsync(database.WeatherReports);
        var hotReports = await hotReportsTask;
        return Results.Ok(hotReports);
    }

    public async Task<IResult> GetFormattedReportsAsync(IWeatherReporter reporter, WeatherDbContext database)
    {
        var formattedReports = await reporter.GetFormattedReportsAsync(database.WeatherReports);
        return Results.Ok(formattedReports);
    }

    public async Task<IResult> PostReportDTOAsync(WeatherReportDTO reportDTO, WeatherDbContext database, IValidator<WeatherReportDTO> validator)
    {
        ValidationResult validationResult = await validator.ValidateAsync(reportDTO);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var addResult = database.Add<WeatherReport>(reportDTO);
        await database.SaveChangesAsync();

        var entity = addResult.Entity;
        return Results.Created($"/weather/reports/{entity.Id}", entity);
    }

    public async Task<IResult> GetFilteredReportAsync(float? minHumidity, IWeatherReporter reporter, WeatherDbContext database)
    {
        // minHumidity comes from the URL: /weather/filter?minHumidity=Value
        var threshold = minHumidity ?? 0.0f;
        var filtered = await reporter.GetMinHumidityReportsAsync(database.WeatherReports, threshold);
        return Results.Ok(filtered);
    }

    public async Task<IResult> GetLocationReportsAsync(string location, IWeatherReporter reporter, WeatherDbContext database)
    {
        if (string.IsNullOrWhiteSpace(location))
        {
            return Results.BadRequest("Location parameter is required.");
        }

        var locationReports = await reporter.GetLocationReportsAsync(database.WeatherReports, location);
        if (locationReports is null || !locationReports.Any())
        {
            return Results.NotFound($"No weather reports found for location: {location}");
        }

        return Results.Ok(locationReports);
    }

    public async Task<IResult> PutReportDTOAsync(int id, WeatherReportDTO reportDTO, WeatherDbContext database, IValidator<WeatherReportDTO> validator)
    {
        ValidationResult validationResult = await validator.ValidateAsync(reportDTO);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }
        var existing = await database.WeatherReports.FindAsync(id);
        if (existing is null)
        {
            return Results.NotFound();
        }
        existing.TemperatureC = reportDTO.TemperatureC;
        existing.Humidity = reportDTO.Humidity;
        existing.Location = reportDTO.Location;
        await database.SaveChangesAsync();
        return Results.NoContent();
    }

    public async Task<IResult> DeleteReportAsync(int id, WeatherDbContext database)
    {
        var existing = await database.WeatherReports.FindAsync(id);
        if (existing is null)
        {
            return Results.NotFound();
        }

        database.Remove(existing);
        await database.SaveChangesAsync();

        return Results.NoContent();
    }
}