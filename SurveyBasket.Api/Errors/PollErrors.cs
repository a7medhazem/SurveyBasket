namespace SurveyBasket.Api.Errors;

public class PollErrors
{
    public static readonly Error PollNotFound =
        new("Poll.NotFound", "No poll was found with the given id");

    public static readonly Error DuplicatedPollTittle =
        new("Poll.DuplicatedTittle", "Another poll with the same tittle is already exists");

}
