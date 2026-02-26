namespace SurveyBasket.Api.Contracts.Results;

public record VotesResponse(
    String VoterName,
    DateTime VoteDate,
    IEnumerable<QuestionAnswerResponse> SelectedAnswers

);