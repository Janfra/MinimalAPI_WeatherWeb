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
        app.MapGet("/weather/hot", async (IWeatherReporter reporter, WeatherDbContext database) =>
        {
            var hotReportsTask = reporter.GetHotReportsAsync(database.WeatherReports);
            var hotReports = await hotReportsTask;
            return Results.Ok(hotReports);
        }); 

        app.MapGet("/weather/format", async (IWeatherReporter reporter, WeatherDbContext database) =>
        {
            var formattedReports = await reporter.GetFormattedReportsAsync(database.WeatherReports);
            return Results.Ok(formattedReports);
        });

        app.MapPost("/weather/reports", async (WeatherReportDTO reportDTO, WeatherDbContext database, IValidator<WeatherReportDTO> validator) =>
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
        });

        app.MapGet("weather/filter", async (float? minHumidity, IWeatherReporter reporter, WeatherDbContext database) =>
        {
            // minHumidity comes from the URL: /weather/filter?minHumidity=Value
            var threshold = minHumidity ?? 0.0f;
            var filtered = await reporter.GetMinHumidityReportsAsync(database.WeatherReports, threshold);
            return Results.Ok(filtered);
        });

        app.MapGet("weather/{location}", async (string location, IWeatherReporter reporter, WeatherDbContext database) =>
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
        });

        app.MapPut("weather/reports/{id:int}", async (int id, WeatherReportDTO updatedReport, WeatherDbContext database, IValidator<WeatherReportDTO> validator) =>
        {
            ValidationResult validationResult = await validator.ValidateAsync(updatedReport);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            var existing = await database.WeatherReports.FindAsync(id);
            if (existing is null) return Results.NotFound();

            existing.TemperatureC = updatedReport.TemperatureC;
            existing.Humidity = updatedReport.Humidity;
            existing.Location = updatedReport.Location;
            await database.SaveChangesAsync();

            return Results.NoContent();
        });

        app.MapDelete("weather/reports/{id:int}", async (int id, WeatherDbContext database) =>
        {
            var existing = await database.WeatherReports.FindAsync(id);
            if (existing is null) return Results.NotFound();
            
            database.Remove(existing);
            await database.SaveChangesAsync();
            
            return Results.NoContent();
        });
    }
}