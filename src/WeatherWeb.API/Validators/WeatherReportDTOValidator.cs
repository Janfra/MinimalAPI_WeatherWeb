namespace WeatherWeb.Validators;

using FluentValidation;
using WeatherWeb.Models;

public class WeatherReportDTOValidator : AbstractValidator<WeatherReportDTO>
{
    public WeatherReportDTOValidator()
    {
        RuleFor(report => report.TemperatureC)
            .InclusiveBetween(-100, 60)
            .WithMessage("Temperature must be between -100°C and 60°C.");
        RuleFor(report => report.Humidity)
            .InclusiveBetween(0, 100)
            .WithMessage("Humidity must be between 0% and 100%.");
        RuleFor(report => report.Location)
            .NotEmpty()
            .WithMessage("Location cannot be empty.")
            .MaximumLength(100)
            .WithMessage("Location cannot exceed 100 characters.");
    }
}