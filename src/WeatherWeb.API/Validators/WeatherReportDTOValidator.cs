namespace WeatherWeb.Validators;

using FluentValidation;
using WeatherWeb.Models;

public class WeatherReportDTOValidator : AbstractValidator<WeatherReportDTO>
{
    public WeatherReportDTOValidator()
    {
        RuleFor(report => report.TemperatureC).SetValidator(new TemperatureCValidator());
        RuleFor(report => report.Humidity).SetValidator(new HumidityValidator());
        RuleFor(report => report.Location).SetValidator(new LocationValidator());
    }
}
