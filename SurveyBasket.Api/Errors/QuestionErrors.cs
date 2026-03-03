namespace SurveyBasket.Api.Errors;

public class QuestionErrors
{
    public static readonly Error QuestionNotFound =
        new("Question.NotFound", "No question was found with the given ID", StatusCodes.Status404NotFound);

    public static readonly Error DuplicatedQuestionContent =
        new("Question.DuplicatedContent", "Another question with the same Content is already exists", StatusCodes.Status409Conflict);

}
