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
                    .WithMessage("Password must be at least 8 characters and include uppercase, lowercase, and a special character.");

        RuleFor(x => x.ConfirmPassword)
                 .NotEmpty()
                 .Equal(x => x.Password)
                 .WithMessage("Passwords do not match.");
    }
}