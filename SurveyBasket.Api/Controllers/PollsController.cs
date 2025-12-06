namespace SurveyBasket.Api.Controllers;

[Route("api/[controller]")] //api/Polls
[ApiController]
public class PollsController(IPollService pollService) : ControllerBase
{
    private readonly IPollService _PollService = pollService;

    [HttpGet("")]
    public IActionResult GetAll()
    {
        return Ok(_PollService.GetAll());
    }


    [HttpGet("{id})")]
    public IActionResult Get(int id)
    {
        var poll = _PollService.Get(id);
        return poll is null ? NotFound() : Ok(poll);
    }

    [HttpPost("")]
    public IActionResult Add(Poll request)
    {
        var newPoll = _PollService.Add(request);
        return CreatedAtAction(nameof(Get), new { id = newPoll.Id }, request);

    }

    [HttpPut("{id}")]

    public IActionResult Update(int id, Poll request)
    {
        var isUpdated = _PollService.Update(id, request);
        if (!isUpdated)
            return NotFound();

        return NoContent();
    }
    [HttpDelete("{id}")]

    public IActionResult Delete(int id)
    {
        var isDelted = _PollService.Delete(id);
        if (!isDelted)
            return NotFound();

        return NoContent();
    }
}