namespace SurveyBasket.Api.Controllers;
[Route("[controller]")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
    private readonly IAuthService _AuthService = authService;

    [HttpPost("")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var authResult = await _AuthService.GetTokenAsync(request.Email, request.Password, cancellationToken);

        return authResult.IsSuccess
            ? Ok(authResult.Value) : authResult.ToProblem();

        //: Problem(statusCode: StatusCodes.Status400BadRequest, title: authResult.Error.Code, detail: authResult.Error.Description);
    }


    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshAsync([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var authResult = await _AuthService.GetRefreshTokenAsync(request.token, request.refreshToken, cancellationToken);

        return authResult.IsSuccess
                   ? Ok(authResult.Value) : authResult.ToProblem();
    }

    [HttpPost("revoke-refresh-token")]
    public async Task<IActionResult> RevokeAsync([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await _AuthService.RevokeRefreshTokenAsync(request.token, request.refreshToken, cancellationToken);

        return result.IsSuccess
            ? Ok() : result.ToProblem();
    }
}
