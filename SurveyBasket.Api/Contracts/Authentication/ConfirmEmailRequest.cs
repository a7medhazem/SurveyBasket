namespace SurveyBasket.Api.Contracts.Authentication;

public record ConfirmEmailRequest(
    [FromQuery] string UserId,
    [FromQuery] string Code
);
