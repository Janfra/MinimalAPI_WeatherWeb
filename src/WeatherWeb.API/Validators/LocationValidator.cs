namespace WeatherWeb.Validators;

using FluentValidation;

public class LocationValidator : AbstractValidator<string>
{
    public LocationValidator()
    {
        RuleFor(location => location)
            .NotEmpty()
            .WithMessage("Location parameter is required.")
            .MaximumLength(100)
            .WithMessage("Location cannot exceed 100 characters.")
            .Matches("^([a-zA-Z0-9]+\\s)*[a-zA-Z0-9]+$")
            .WithMessage("Only alphanumeric characters with a single whitespace in between are allowed.");
    }
}   