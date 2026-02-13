namespace SurveyBasket.Api.Errors;

public class QuestionErrors
{
    public static readonly Error QuestionNotFound =
        new("Question.NotFound", "No question was found with the given ID");

    public static readonly Error DuplicatedQuestionContent =
        new("Question.DuplicatedContent", "Another question with the same Content is already exists");

    public static readonly Error QuestionNotActive =
        new("Question.NotActive", "This question is not active, so it cannot be updated");

}
