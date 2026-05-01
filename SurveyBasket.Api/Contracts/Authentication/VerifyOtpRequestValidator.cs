namespace SurveyBasket.Api.Contracts.Authentication;

public class VerifyOtpRequestValidator : AbstractValidator<VerifyOtpRequest>
{
    public VerifyOtpRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.OtpCode)
            .NotEmpty()
            .Length(6)
            .Matches(RegexPatterns.Otp)
            .WithMessage("OTP must be exactly 6 digits");
    }
}