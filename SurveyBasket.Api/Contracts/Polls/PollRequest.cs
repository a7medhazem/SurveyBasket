namespace SurveyBasket.Api.Contracts.Polls;

public record PollRequest(
     string Tittle,
     string Summary,
     bool IsPublished,
     DateOnly StartsAt,
     DateOnly EndsAt
);