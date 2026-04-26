namespace SurveyBasket.Api.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize]
public class AccountController(IUserService userService) : ControllerBase
{
    private readonly IUserService _userService = userService;

    [HttpGet("profile")]
    public async Task<IActionResult> Info(CancellationToken cancellationToken)
    {
        var result = await _userService.GetProfileAsync(User.GetUserId()!, cancellationToken);

        return Ok(result.Value);
    }

    [HttpPut("update-profile")]
    public async Task<IActionResult> Update([FromBody] UpdateProfileRequest request)
    {
        var result = await _userService.UpdateProfileAsync(User.GetUserId()!, request);

        return NoContent();
    }


    [HttpPut("change-password")]
    public async Task<IActionResult> CahngePassword([FromBody] ChangePasswordRequest request)
    {
        var result = await _userService.ChangePasswordAsync(User.GetUserId()!, request);

        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

}
