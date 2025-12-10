namespace SurveyBasket.Api.Contracts.Validations;

public class CreatePollRequestValidator : AbstractValidator<CreatePollRequest>
{
    public CreatePollRequestValidator()
    {
        RuleFor(x => x.Tittle)
            .NotEmpty()
            .Length(3, 100);

        RuleFor(x=>x.Description)
            .NotEmpty()
            .Length(3, 1000);

    }
}
