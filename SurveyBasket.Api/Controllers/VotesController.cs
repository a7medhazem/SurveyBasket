namespace SurveyBasket.Api.Controllers;

[Route("api/polls/{PollId}/vote")]
[ApiController]
[Authorize]
public class VotesController(IQuestionService questionService, IVoteService voteService) : ControllerBase
{
    private readonly IQuestionService _questionService = questionService;
    private readonly IVoteService _voteService = voteService;

    [HttpGet("")]
    public async Task<IActionResult> Start([FromRoute] int PollId, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        var result = await _questionService.GetAvailableAsync(PollId, userId!, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("")]
    public async Task<IActionResult> Vote([FromRoute] int PollId, [FromBody] VoteRequest request, CancellationToken cancellationToken)
    {
        var result = await _voteService.AddAsync(PollId, User.GetUserId()!, request, cancellationToken);

        return result.IsSuccess
            ? Created() : result.ToProblem();
    }
}
