namespace SurveyBasket.Api.Errors;

public class QuestionErrors
{
    public static readonly Error QuestionNotFound =
        new("Question.NotFound", "No question was found with the given ID", StatusCodes.Status404NotFound);

    public static readonly Error DuplicatedQuestionContent =
        new("Question.DuplicatedContent", "Another question with the same Content is already exists", StatusCodes.Status409Conflict);

    public static readonly Error QuestionNotActive =
        new("Question.NotActive", "This question is not active, so it cannot be updated", StatusCodes.Status404NotFound);

}
