namespace SurveyBasket.Api.Errors;

public static class VoteErrors
{
    public static readonly Error DuplicatedVote =
        new("Vote.DuplicatedVote", "User already voted before for this poll");
    public static readonly Error InvalidQuestions =
        new("Vote.InvalidQuestions", "Invalid questions");
}