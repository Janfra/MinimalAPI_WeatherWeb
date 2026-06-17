namespace WeatherWeb.EndpointMappers;

using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
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
        var reportDTO = context.GetArgument<WeatherReportRequest>(0);
        var reportValidator = context.GetRequiredService<IValidator<WeatherReportRequest>>();
        var validationResult = await reportValidator.ValidateAsync(reportDTO);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }
        return await next(context);
    }

    public static async Task<Ok<IReadOnlyList<WeatherReportResponse>>> GetHotReportsAsync(IWeatherReportService reportService)
    {
        var hotReportsTask = reportService.GetHotReportsAsync();
        var hotReports = await hotReportsTask;
        return TypedResults.Ok(hotReports);
    }

    public static async Task<Ok<IReadOnlyList<string>>> GetFormattedReportsAsync(IWeatherReportService reportService)
    {
        var formattedReports = await reportService.GetFormattedReportsAsync();
        return TypedResults.Ok(formattedReports);
    }

    public static async Task<Created<WeatherReportResponse>> PostReportDTOAsync(WeatherReportRequest reportDTO, IWeatherReportService reportService)
    {
        var response = await reportService.AddWeatherReportAsync(reportDTO);
        return TypedResults.Created($"/weather/reports/{response.Id}", response);
    }

    public static async Task<Ok<IReadOnlyList<WeatherReportResponse>>> GetFilteredReportAsync(float? minHumidity, IWeatherReportService reportService)
    {
        // minHumidity comes from the URL: /weather/filter?minHumidity=Value
        var threshold = minHumidity ?? 0.0f;
        var filtered = await reportService.GetMinHumidityReportsAsync(threshold);
        return TypedResults.Ok(filtered);
    }

    public static async Task<Results<Ok<IReadOnlyList<WeatherReportResponse>>, BadRequest<string>, NotFound<string>>> GetLocationReportsAsync(string location, IWeatherReportService reportService)
    {
        var locationReports = await reportService.GetLocationReportsAsync(location);
        if (locationReports is null || locationReports.Count == 0)
        {
            return TypedResults.NotFound($"No weather reports found for location: {location}");
        }

        return TypedResults.Ok(locationReports);
    }

    public static async Task<Results<NoContent, NotFound>> PutReportDTOAsync(int id, WeatherReportRequest reportDTO, IWeatherReportService reportService)
    {
        var existing = await reportService.PutWeatherReportAsync(id, reportDTO);
        if (existing is null)
        {
            return TypedResults.NotFound();
        }
        return TypedResults.NoContent();
    }

    public static async Task<Results<NoContent, NotFound>> DeleteReportAsync(int id, IWeatherReportService reportService)
    {
        var existing = await reportService.DeleteWeatherReportAsync(id);
        if (existing is null)
        {
            return TypedResults.NotFound();
        }
        return TypedResults.NoContent();
    }
}