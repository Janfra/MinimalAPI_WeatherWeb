namespace WeatherWeb.Mappers;

using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;
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

    public async Task<Ok<IReadOnlyList<WeatherReport>>> GetHotReportsAsync(IWeatherReporter reporter, IWeatherDbContext database)
    {
        var hotReportsTask = reporter.GetHotReportsAsync(database.WeatherReports);
        var hotReports = await hotReportsTask;
        return TypedResults.Ok(hotReports);
    }

    public async Task<Ok<IReadOnlyList<string>>> GetFormattedReportsAsync(IWeatherReporter reporter, IWeatherDbContext database)
    {
        var formattedReports = await reporter.GetFormattedReportsAsync(database.WeatherReports);
        return TypedResults.Ok(formattedReports);
    }

    public async Task<Results<Created<WeatherReport>, ValidationProblem>> PostReportDTOAsync(WeatherReportDTO reportDTO, IWeatherDbContext database, IValidator<WeatherReportDTO> validator)
    {
        ValidationResult validationResult = await validator.ValidateAsync(reportDTO);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        var entityAdded = database.Add(reportDTO.ToEntity());
        await database.SaveChangesAsync();
        return TypedResults.Created($"/weather/reports/{entityAdded.Id}", entityAdded);
    }

    public async Task<Ok<IReadOnlyList<WeatherReport>>> GetFilteredReportAsync(float? minHumidity, IWeatherReporter reporter, IWeatherDbContext database)
    {
        // minHumidity comes from the URL: /weather/filter?minHumidity=Value
        var threshold = minHumidity ?? 0.0f;
        var filtered = await reporter.GetMinHumidityReportsAsync(database.WeatherReports, threshold);
        return TypedResults.Ok(filtered);
    }

    public async Task<Results<Ok<IReadOnlyList<WeatherReport>>, BadRequest<string>, NotFound<string>>> GetLocationReportsAsync(string location, IWeatherReporter reporter, IWeatherDbContext database)
    {
        if (string.IsNullOrWhiteSpace(location))
        {
            return TypedResults.BadRequest("Location parameter is required.");
        }

        var locationReports = await reporter.GetLocationReportsAsync(database.WeatherReports, location);
        if (locationReports is null || locationReports.Count == 0)
        {
            return TypedResults.NotFound($"No weather reports found for location: {location}");
        }

        return TypedResults.Ok(locationReports);
    }

    public async Task<Results<NoContent, ValidationProblem, NotFound>> PutReportDTOAsync(int id, WeatherReportDTO reportDTO, IWeatherDbContext database, IValidator<WeatherReportDTO> validator)
    {
        ValidationResult validationResult = await validator.ValidateAsync(reportDTO);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }
        var existing = await database.WeatherReports.FindAsync(id);
        if (existing is null)
        {
            return TypedResults.NotFound();
        }
        existing.TemperatureC = reportDTO.TemperatureC;
        existing.Humidity = reportDTO.Humidity;
        existing.Location = reportDTO.Location;
        await database.SaveChangesAsync();
        return TypedResults.NoContent();
    }

    public async Task<Results<NoContent, NotFound>> DeleteReportAsync(int id, IWeatherDbContext database)
    {
        var existing = await database.WeatherReports.FindAsync(id);
        if (existing is null)
        {
            return TypedResults.NotFound();
        }

        database.Remove(existing);
        await database.SaveChangesAsync();

        return TypedResults.NoContent();
    }
}