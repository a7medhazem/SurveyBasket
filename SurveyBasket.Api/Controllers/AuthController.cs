namespace SurveyBasket.Api.Controllers;
[Route("[controller]")]
[ApiController]
public class AuthController(IAuthService authService, ILogger<AuthService> logger) : ControllerBase
{
    private readonly IAuthService _authService = authService;
    private readonly ILogger<AuthService> _logger = logger;

    [HttpPost("")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Logging with email: {email} and password:{password}", request.Email, request.Password);

        var authResult = await _authService.GetTokenAsync(request.Email, request.Password, cancellationToken);

        return authResult.IsSuccess
            ? Ok(authResult.Value) : authResult.ToProblem();

        //: Problem(statusCode: StatusCodes.Status400BadRequest, title: authResult.Error.Code, detail: authResult.Error.Description);
    }


    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var authResult = await _authService.GetRefreshTokenAsync(request.Token, request.RefreshToken, cancellationToken);

        return authResult.IsSuccess
                   ? Ok(authResult.Value) : authResult.ToProblem();
    }


    [HttpPost("revoke-refresh-token")]
    public async Task<IActionResult> Revoke([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RevokeRefreshTokenAsync(request.Token, request.RefreshToken, cancellationToken);

        return result.IsSuccess
            ? Ok() : result.ToProblem();
    }


    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(request, cancellationToken);

        return result.IsSuccess
            ? Ok() : result.ToProblem();
    }


    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] ConfirmEmailRequest request)
    {
        var result = await _authService.ConfirmEmailAsync(request);

        return result.IsSuccess
            ? Content(GetConfirmedHtml(true), "text/html")
            : Content(GetConfirmedHtml(false), "text/html");
    }

    [HttpPost("resend-confirmation-email")]
    public async Task<IActionResult> ResendConfirmation([FromBody] ResendConfirnationEmailRequest request)
    {
        var result = await _authService.ResendConfirnationEmailAsync(request);

        return result.IsSuccess
            ? Ok() : result.ToProblem();
    }




    private static string GetConfirmedHtml(bool success)
    {
        return success ? """
    <!DOCTYPE html>
    <html lang="en">
    <head>
        <meta charset="UTF-8">
        <meta name="viewport" content="width=device-width, initial-scale=1.0">
        <title>Email Confirmed</title>
        <style>
            * { margin: 0; padding: 0; box-sizing: border-box; }
            body {
                font-family: 'Segoe UI', sans-serif;
                background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                min-height: 100vh;
                display: flex;
                align-items: center;
                justify-content: center;
            }
            .card {
                background: white;
                border-radius: 20px;
                padding: 50px 40px;
                text-align: center;
                max-width: 480px;
                width: 90%;
                box-shadow: 0 20px 60px rgba(0,0,0,0.2);
            }
            .icon { font-size: 72px; margin-bottom: 20px; }
            h1 { color: #2d3748; font-size: 28px; margin-bottom: 12px; }
            p { color: #718096; font-size: 16px; line-height: 1.6; margin-bottom: 30px; }
            .badge {
                background: #f0fff4;
                color: #38a169;
                border: 1px solid #9ae6b4;
                border-radius: 50px;
                padding: 8px 24px;
                font-size: 14px;
                font-weight: 600;
                display: inline-block;
            }
        </style>
    </head>
    <body>
        <div class="card">
            <div class="icon">✅</div>
            <h1>Email Confirmed!</h1>
            <p>Your email has been verified successfully.<br>You can now log in to your account.</p>
            <span class="badge">✔ Verification Complete</span>
        </div>
    </body>
    </html>
    """
        : """
    <!DOCTYPE html>
    <html lang="en">
    <head>
        <meta charset="UTF-8">
        <meta name="viewport" content="width=device-width, initial-scale=1.0">
        <title>Confirmation Failed</title>
        <style>
            * { margin: 0; padding: 0; box-sizing: border-box; }
            body {
                font-family: 'Segoe UI', sans-serif;
                background: linear-gradient(135deg, #fc4a1a 0%, #f7b733 100%);
                min-height: 100vh;
                display: flex;
                align-items: center;
                justify-content: center;
            }
            .card {
                background: white;
                border-radius: 20px;
                padding: 50px 40px;
                text-align: center;
                max-width: 480px;
                width: 90%;
                box-shadow: 0 20px 60px rgba(0,0,0,0.2);
            }
            .icon { font-size: 72px; margin-bottom: 20px; }
            h1 { color: #2d3748; font-size: 28px; margin-bottom: 12px; }
            p { color: #718096; font-size: 16px; line-height: 1.6; margin-bottom: 30px; }
            .badge {
                background: #fff5f5;
                color: #e53e3e;
                border: 1px solid #feb2b2;
                border-radius: 50px;
                padding: 8px 24px;
                font-size: 14px;
                font-weight: 600;
                display: inline-block;
            }
        </style>
    </head>
    <body>
        <div class="card">
            <div class="icon">❌</div>
            <h1>Confirmation Failed</h1>
            <p>The confirmation link is invalid or has expired.<br>Please request a new confirmation email.</p>
            <span class="badge">✖ Verification Failed</span>
        </div>
    </body>
    </html>
    """;
    }
}
