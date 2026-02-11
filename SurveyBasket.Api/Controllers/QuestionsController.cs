using Azure.Core;

namespace SurveyBasket.Api.Controllers;

[Route("api/polls/{pollId}/[controller]")]
[ApiController]
[Authorize]
public class QuestionsController(IQuestionService questionService) : ControllerBase
{
    private readonly IQuestionService _QuestionService = questionService;


    [HttpGet("")]
    public async Task<IActionResult> GetAll([FromRoute] int pollId, CancellationToken cancellationToken)
    {
        var result = await _QuestionService.GetAllAsync(pollId, cancellationToken);

        return result.IsSuccess ?
            Ok(result.Value) : result.ToProblem(statusCode: StatusCodes.Status404NotFound);
    }


    [HttpGet("{id}")]
    public async Task<IActionResult> Get() => Ok();


    [HttpPost("")]
    public async Task<IActionResult> Add([FromRoute] int pollId, [FromBody] QuestionRequest request, CancellationToken cancellationToken)
    {
        var result = await _QuestionService.AddAsync(pollId, request, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { pollId, result.Value.Id }, result.Value)
            : result.Error.Code == QuestionErrors.DuplicatedQuestionContent.Code
            ? result.ToProblem(statusCode: StatusCodes.Status409Conflict)
            : result.ToProblem(statusCode: StatusCodes.Status404NotFound);
    }

}
