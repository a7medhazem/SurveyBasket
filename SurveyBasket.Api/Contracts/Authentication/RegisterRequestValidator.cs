namespace SurveyBasket.Api.Contracts.Authentication;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FirstName)
                    .NotEmpty()
                    .Length(3, 100);

        RuleFor(x => x.LastName)
                    .NotEmpty()
                    .Length(3, 100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
                    .NotEmpty()
                    .Matches(RegexPatterns.Password)
                    .WithMessage("Password should be at least 8 digits and should contains Lowercase, Uppercase and NonAlphanumeric");

        RuleFor(x => x.ConfirmPassword)
                 .NotEmpty()
                 .Equal(x => x.Password)
                 .WithMessage("Passwords do not match.");
    }
}