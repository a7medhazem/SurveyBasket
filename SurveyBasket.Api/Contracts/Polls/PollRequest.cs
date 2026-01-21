namespace SurveyBasket.Api.Contracts.Polls;

public record PollRequest(
     string Tittle,
     string Summary,
     DateOnly StartsAt,
     DateOnly EndsAt
);