namespace WeatherWeb.EndpointMappers;

using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using WeatherWeb.Data;
using WeatherWeb.Extensions;
using WeatherWeb.Models;
using WeatherWeb.Services.Reporter;
using WeatherWeb.Validators;

/// <remarks>
/// This class does not implement <c>IEndpointMapper</c> and instead use an extension method to map this specific mapper for simplicity.
/// </remarks>
public static class WeatherRouteMapper
{
    public static void MapWeatherEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        // Could create an endpoint filter class in order to set which spicific argument index the validators must validate, but for simplicity we will just implement the specific validator for each endpoint that needs it due to the small number of endpoints and validators in this example.

        endpointRouteBuilder.MapGet("/weather/hot", GetHotReportsAsync);

        endpointRouteBuilder.MapGet("/weather/format", GetFormattedReportsAsync);

        endpointRouteBuilder.MapPost("/weather/reports", PostReportDTOAsync)
            .AddEndpointFilter(ValidateReportDTO);

        endpointRouteBuilder.MapGet("weather/filter", GetFilteredReportAsync);

        endpointRouteBuilder.MapGet("weather/{location}", GetLocationReportsAsync)
            .AddEndpointFilter(ValidateLocation);

        endpointRouteBuilder.MapPut("weather/reports/{id:int}", PutReportDTOAsync);

        endpointRouteBuilder.MapDelete("weather/reports/{id:int}", DeleteReportAsync);
    }

    public static async ValueTask<object?> ValidateLocation(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var location = context.GetArgument<string>(0);
        var locationValidator = context.GetRequiredService<LocationValidator>();
        var validationResult = await locationValidator.ValidateAsync(location);

        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        return await next(context);
    }

    public static async ValueTask<object?> ValidateReportDTO(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var reportDTO = context.GetArgument<WeatherReportDTO>(0);
        var reportValidator = context.GetRequiredService<IValidator<WeatherReportDTO>>();
        var validationResult = await reportValidator.ValidateAsync(reportDTO);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }
        return await next(context);
    }

    public static async Task<Ok<IReadOnlyList<WeatherReport>>> GetHotReportsAsync(IWeatherReporter reporter, IWeatherDbContext database)
    {
        var hotReportsTask = reporter.GetHotReportsAsync(database.WeatherReports);
        var hotReports = await hotReportsTask;
        return TypedResults.Ok(hotReports);
    }

    public static async Task<Ok<IReadOnlyList<string>>> GetFormattedReportsAsync(IWeatherReporter reporter, IWeatherDbContext database)
    {
        var formattedReports = await reporter.GetFormattedReportsAsync(database.WeatherReports);
        return TypedResults.Ok(formattedReports);
    }

    public static async Task<Created<WeatherReport>> PostReportDTOAsync(WeatherReportDTO reportDTO, IWeatherDbContext database)
    {
        var entityAdded = database.Add(reportDTO.ToEntity());
        await database.SaveChangesAsync();
        return TypedResults.Created($"/weather/reports/{entityAdded.Id}", entityAdded);
    }

    public static async Task<Ok<IReadOnlyList<WeatherReport>>> GetFilteredReportAsync(float? minHumidity, IWeatherReporter reporter, IWeatherDbContext database)
    {
        // minHumidity comes from the URL: /weather/filter?minHumidity=Value
        var threshold = minHumidity ?? 0.0f;
        var filtered = await reporter.GetMinHumidityReportsAsync(database.WeatherReports, threshold);
        return TypedResults.Ok(filtered);
    }

    public static async Task<Results<Ok<IReadOnlyList<WeatherReport>>, BadRequest<string>, NotFound<string>>> GetLocationReportsAsync(string location, IWeatherReporter reporter, IWeatherDbContext database)
    {
        var locationReports = await reporter.GetLocationReportsAsync(database.WeatherReports, location);
        if (locationReports is null || locationReports.Count == 0)
        {
            return TypedResults.NotFound($"No weather reports found for location: {location}");
        }

        return TypedResults.Ok(locationReports);
    }

    public static async Task<Results<NoContent, NotFound>> PutReportDTOAsync(int id, WeatherReportDTO reportDTO, IWeatherDbContext database)
    {
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

    public static async Task<Results<NoContent, NotFound>> DeleteReportAsync(int id, IWeatherDbContext database)
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