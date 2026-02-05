using Microsoft.AspNetCore.Authorization;
using System.Reflection.Metadata.Ecma335;

namespace SurveyBasket.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]

public class PollsController(IPollService pollService) : ControllerBase
{
    private readonly IPollService _PollService = pollService;

    [HttpGet("")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var polls = await _PollService.GetAllAsync(cancellationToken);
        var response = polls.Adapt<IEnumerable<PollResponse>>();
        return Ok(response);
    }


    [HttpGet("{id}")]

    public async Task<IActionResult> Get([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await _PollService.GetAsync(id, cancellationToken);


        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem(statusCode: StatusCodes.Status404NotFound);
    }



    [HttpPost("")]
    public async Task<IActionResult> Add([FromBody] PollRequest request, CancellationToken cancellationToken)
    {
        var result = await _PollService.AddAsync(request, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value)
            : result.ToProblem(statusCode: StatusCodes.Status409Conflict);
    }


    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] PollRequest request, CancellationToken cancellationToken)
    {
        var result = await _PollService.UpdateAsync(id, request, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.Error.Code == PollErrors.PollNotFound.Code
            ? result.ToProblem(statusCode: StatusCodes.Status404NotFound)
            : result.ToProblem(statusCode: StatusCodes.Status409Conflict);
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await _PollService.DeleteAsync(id, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.ToProblem(statusCode: StatusCodes.Status404NotFound);
    }


    [HttpPut("{id}/togglePublish")]
    public async Task<IActionResult> TogglePublish([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await _PollService.TogglePublishStatusAsync(id, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.ToProblem(statusCode: StatusCodes.Status404NotFound);
    }
}