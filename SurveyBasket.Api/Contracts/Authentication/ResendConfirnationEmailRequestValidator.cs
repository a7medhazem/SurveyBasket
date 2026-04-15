namespace SurveyBasket.Api.Contracts.Authentication;

public class ResendConfirnationEmailRequestValidator : AbstractValidator<ResendConfirnationEmailRequest>
{
    public ResendConfirnationEmailRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }
}