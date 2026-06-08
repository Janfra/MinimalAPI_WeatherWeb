namespace WeatherWeb.ServiceConfigurators;

using FluentValidation;
using Microsoft.EntityFrameworkCore;
using WeatherWeb.Data;
using WeatherWeb.Models;
using WeatherWeb.Services.Formatter;
using WeatherWeb.Services.Reporter;
using WeatherWeb.Validators;

public static class WeatherServiceConfigurator
{
    private const string _weatherConnectionStringKey = "DefaultConnection";

    public static void AddWeatherServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IWeatherFormatter, StandardWeatherFormatter>();
        services.AddScoped<IWeatherReporter, WeatherReporter>();
        services.AddScoped<LocationValidator>();
        services.AddDbContext<WeatherDbContext>(
            options => options.UseSqlite(configuration.GetConnectionString(_weatherConnectionStringKey) ?? throw new InvalidOperationException($"Connection string `{_weatherConnectionStringKey}` not found."))
            );
        services.AddScoped<IWeatherDbContext>(
            serviceProvider => serviceProvider.GetRequiredService<WeatherDbContext>()
        );
        services.AddScoped<IValidator<WeatherReportDTO>, WeatherReportDTOValidator>();
    }
}
