using System.Security.Claims;

namespace SurveyBasket.Api.Controllers;

[Route("api/polls/{PollId}/vote")]
[ApiController]
[Authorize]
public class VotesController(IQuestionService questionService) : ControllerBase
{
    private readonly IQuestionService _questionService = questionService;

    [HttpGet("")]
    public async Task<IActionResult> Start([FromRoute] int PollId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _questionService.GetAvailableAsync(PollId, userId!, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.Error.Code == VoteErrors.DuplicatedVote.Code
            ? result.ToProblem(StatusCodes.Status409Conflict)
            : result.ToProblem(StatusCodes.Status404NotFound);
    }
}
