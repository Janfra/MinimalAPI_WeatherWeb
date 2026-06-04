namespace WeatherWeb.Validators;

using FluentValidation;

public class TemperatureCValidator : AbstractValidator<int>
{
    public TemperatureCValidator()
    {
        RuleFor(temp => temp)
            .InclusiveBetween(-100, 60)
            .WithMessage("Temperature must be between -100°C and 60°C.");
    }
}
