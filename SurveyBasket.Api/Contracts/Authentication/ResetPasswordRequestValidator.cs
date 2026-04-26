namespace SurveyBasket.Api.Contracts.Authentication;

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Code)
            .NotEmpty();

        RuleFor(x => x.NewPassword)
                    .NotEmpty()
                    .Matches(RegexPatterns.Password)
                    .WithMessage("Password must be at least 8 characters and include uppercase, lowercase, and a special character.");

        RuleFor(x => x.ConfirmPassword)
                 .NotEmpty()
                 .Equal(x => x.NewPassword)
                 .WithMessage("Passwords do not match.");
    }
}