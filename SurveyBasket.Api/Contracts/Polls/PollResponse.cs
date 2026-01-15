namespace SurveyBasket.Api.Contracts.Polls;

public record PollResponse(
     int Id,
     string Tittle,
     string Summary,
     bool IsPublished,
     DateOnly StartsAt,
     DateOnly EndsAt 
);
