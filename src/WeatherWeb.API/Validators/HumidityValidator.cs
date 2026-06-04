namespace WeatherWeb.Validators;

using FluentValidation;

public class HumidityValidator : AbstractValidator<float>
{
    public HumidityValidator()
    {
        RuleFor(humidity => humidity)
            .InclusiveBetween(0.0f, 100.0f)
            .WithMessage("Humidity must be between 0% and 100%.");
    }
}