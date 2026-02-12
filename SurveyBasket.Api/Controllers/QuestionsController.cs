using Azure.Core;
using SurveyBasket.Api.Entites;

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
    public async Task<IActionResult> Get([FromRoute] int pollId, [FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await _QuestionService.GetAsync(pollId, id, cancellationToken);

        return result.IsSuccess ?
            Ok(result.Value) : result.ToProblem(statusCode: StatusCodes.Status404NotFound);
    }


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

    [HttpPut("{id}/toggleStatus")]
    public async Task<IActionResult> ToggleStatus([FromRoute] int pollId, [FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await _QuestionService.ToggleStatusAsync(pollId, id, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.ToProblem(statusCode: StatusCodes.Status404NotFound);
    }

}
