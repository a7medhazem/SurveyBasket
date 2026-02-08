namespace SurveyBasket.Api.Contracts.Questions;

public record QuestionRequest(
    String Content,
    List<String> Answers

);
