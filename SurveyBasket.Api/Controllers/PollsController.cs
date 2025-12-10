namespace SurveyBasket.Api.Controllers;

[Route("api/[controller]")] //api/Polls
[ApiController]
public class PollsController(IPollService pollService) : ControllerBase
{
    private readonly IPollService _PollService = pollService;

    [HttpGet("")]
    public IActionResult GetAll()
    {
        var polls = _PollService.GetAll();
        var response = polls.Adapt<IEnumerable<PollResponse>>();
        return Ok(response);
    }


    [HttpGet("{id}")]
    public IActionResult Get([FromRoute] int id)
    {
        var poll = _PollService.Get(id);

        if (poll is null)
            return NotFound();

        var response = poll.Adapt<PollResponse>();

        return Ok(response);
    }


    [HttpPost("")]
    public IActionResult Add([FromBody] CreatePollRequest request)
    {
        var newPoll = _PollService.Add(request.Adapt<Poll>());
        return CreatedAtAction(nameof(Get), new { id = newPoll.Id }, request);

    }

    [HttpPut("{id}")]
    public IActionResult Update([FromRoute] int id, [FromBody] CreatePollRequest request)
    {
        var isUpdated = _PollService.Update(id, request.Adapt<Poll>());
        if (!isUpdated)
            return NotFound();

        return NoContent();
    }


    [HttpDelete("{id}")]
    public IActionResult Delete([FromRoute] int id)
    {
        var isDelted = _PollService.Delete(id);
        if (!isDelted)
            return NotFound();

        return NoContent();
    }
}