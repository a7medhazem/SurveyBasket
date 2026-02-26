namespace SurveyBasket.Api.Contracts.Results;

public record PollVotesResponse(
    String Tittle,
    IEnumerable<VotesResponse> Votes
);