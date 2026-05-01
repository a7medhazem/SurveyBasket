namespace SurveyBasket.Api.Contracts.Authentication;

public class ResendOtpRequestValidator : AbstractValidator<ResendOtpRequest>
{
    public ResendOtpRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }
}
