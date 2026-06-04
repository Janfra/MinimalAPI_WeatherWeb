namespace WeatherWeb.ServiceConfigurators;

using FluentValidation;
using WeatherWeb.Data;
using WeatherWeb.Models;
using WeatherWeb.Services.Formatter;
using WeatherWeb.Services.Reporter;
using WeatherWeb.Validators;

public static class WeatherServiceConfigurator
{
    public static void AddWeatherServices(this IServiceCollection services)
    {
        services.AddSingleton<IWeatherFormatter, StandardWeatherFormatter>();
        services.AddScoped<IWeatherReporter, WeatherReporter>();
        services.AddScoped<LocationValidator>();
        services.AddDbContext<WeatherDbContext>();
        services.AddScoped<IWeatherDbContext>(
            serviceProvider => serviceProvider.GetRequiredService<WeatherDbContext>()
        );
        services.AddScoped<IValidator<WeatherReportDTO>, WeatherReportDTOValidator>();
    }
}
