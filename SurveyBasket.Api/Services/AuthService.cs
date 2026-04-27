namespace SurveyBasket.Api.Services;

public class AuthService(UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ILogger<AuthService> logger,
    IJwtProvider jwtProvider,
    IEmailSender emailSender,
    IOptions<AppSettings> appSettings,
     ApplicationDbContext context) : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;//to use identity methods
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;// Handles the sign-in process efficiently
    private readonly ILogger<AuthService> _logger = logger;
    private readonly IJwtProvider _jwtProvider = jwtProvider;//to call GenerateToken method
    private readonly IEmailSender _emailSender = emailSender;
    private readonly IOptions<AppSettings> _appSettings = appSettings;
    private readonly ApplicationDbContext _context = context;

    private readonly int _refreshTokenExpiryDays = 14;

    public async Task<Result<AuthResponse>> GetTokenAsync(string email, string password, CancellationToken cancellationToken)
    {
        // check user
        if (await _userManager.FindByEmailAsync(email) is not { } user)
            return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);

        // check password 

        // var isValidPassword = await _userManager.CheckPasswordAsync(user, password);
        // if (!isValidPassword)
        //  return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);

        var result = await _signInManager.PasswordSignInAsync(user, password, false, false);


        if (result.Succeeded)
        {
            //generate JWT token
            (string token, int expiresIn) = _jwtProvider.GenerateToken(user);

            //generate Refresh Token 
            var refreshToken = GenerateRefreshToken();
            var refreshTokenExpiration = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);

            user.RefreshTokens.Add(
                new RefreshToken()
                {
                    Token = refreshToken,
                    ExpiresOn = refreshTokenExpiration

                });
            await _userManager.UpdateAsync(user);

            var response = new AuthResponse(user.Id, user.Email!, user.FirstName, user.LastName, token, expiresIn, refreshToken, refreshTokenExpiration);

            //return new auth response
            return Result.Success(response);

        }

        // if sign in proccess not succeded
        return Result.Failure<AuthResponse>(result.IsNotAllowed ? UserErrors.EmailNotConfirmed : UserErrors.InvalidCredentials);

    }


    public async Task<Result<AuthResponse>> GetRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default)
    {
        var userId = _jwtProvider.ValidateToken(token);//return user id or null

        if (userId is null)
            return Result.Failure<AuthResponse>(UserErrors.InvalidJwtToken);


        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            return Result.Failure<AuthResponse>(UserErrors.InvalidJwtToken);


        var UserRefreshToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken && x.IsActive);

        if (UserRefreshToken is null)
            return Result.Failure<AuthResponse>(UserErrors.InvalidJwtToken);


        UserRefreshToken.RevokedOn = DateTime.UtcNow;

        // After validating the access and refresh tokens for the current user,
        // generate new access and refresh tokens.

        //generate JWT token
        (string newToken, int expiresIn) = _jwtProvider.GenerateToken(user);

        //generate Refresh Token 
        var newRefreshToken = GenerateRefreshToken();
        var refreshTokenExpiration = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);

        user.RefreshTokens.Add(
            new RefreshToken()
            {
                Token = newRefreshToken,
                ExpiresOn = refreshTokenExpiration
            });
        await _userManager.UpdateAsync(user);

        //return new auth response
        var response = new AuthResponse(user.Id, user.Email!, user.FirstName, user.LastName, newToken, expiresIn, newRefreshToken, refreshTokenExpiration);

        return Result.Success(response);

    }


    public async Task<Result> RevokeRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default)
    {
        var userId = _jwtProvider.ValidateToken(token);//return user id or null

        if (userId is null)
            return Result.Failure(UserErrors.InvalidJwtToken);


        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            return Result.Failure(UserErrors.InvalidJwtToken);

        var UserRefreshToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken && x.IsActive);

        if (UserRefreshToken is null)
            return Result.Failure(UserErrors.InvalidRefreshToken);


        UserRefreshToken.RevokedOn = DateTime.UtcNow;

        await _userManager.UpdateAsync(user);

        return Result.Success();//refresh token is revoked successfuly

    }

    public async Task<Result> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        // check duplicate email
        var emailExists = await _userManager.Users.AnyAsync(x => x.Email == request.Email, cancellationToken);

        if (emailExists)
            return Result.Failure(UserErrors.DuplicatedEmail);

        // create user
        var user = request.Adapt<ApplicationUser>();

        var result = await _userManager.CreateAsync(user, request.Password);

        // Generate email confirmation token
        if (result.Succeeded)
        {
            // Generate email confirmation token
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            // Encode the token to be URL-safe
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            // Log the confirmation code (for development/testing only)
            _logger.LogInformation("Confirmation code: {code}", code);

            await SendConfirmationEmail(user, code);

            return Result.Success();
        }

        var error = result.Errors.First();

        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));

    }

    public async Task<Result> ConfirmEmailAsync(ConfirmEmailRequest request)
    {
        // 1. Retrieve user by Id and validate existence
        if (await _userManager.FindByIdAsync(request.UserId) is not { } user)
            return Result.Failure(UserErrors.InvalidCode);

        // 2. Check if email is already confirmed
        if (user.EmailConfirmed)
            return Result.Failure(UserErrors.DuplicatedConfirmation);

        // 3. Decode the token(code) from URL format to normal string
        var code = request.Code;

        try
        {
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        }
        catch (FormatException)
        {
            return Result.Failure(UserErrors.InvalidCode);
        }

        // 4. Verifies the token is valid for this user (generated with same user data & security stamp)
        var result = await _userManager.ConfirmEmailAsync(user, code);


        // 5. Return result (success or failure)
        if (result.Succeeded)
            return Result.Success();

        var error = result.Errors.First();

        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }
    public async Task<Result> ResendConfirnationEmailAsync(ResendConfirnationEmailRequest request)
    {
        // 1. Retrieve user email and validate existence
        if (await _userManager.FindByEmailAsync(request.Email) is not { } user)
            return Result.Success();

        // 2. Check if email is already confirmed
        if (user.EmailConfirmed)
            return Result.Failure(UserErrors.DuplicatedConfirmation);


        // 3. Generate and encode a URL-safe email confirmation token

        // Generate email confirmation token
        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        // Encode the token to be URL-safe
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        // 4. Log the token and send confirmation email to the user
        _logger.LogInformation("Resend confirmation token for {Email}: {Token}", user.Email, code);

        await SendConfirmationEmail(user, code);

        return Result.Success();
    }

    public async Task<Result> SendResetPasswordCodeAsync(string email)
    {
        if (await _userManager.FindByEmailAsync(email) is not { } user)
            return Result.Success();

        if (!user.EmailConfirmed)
            return Result.Failure(UserErrors.DuplicatedConfirmation);


        // 1. Generate OTP (6 digits - cryptographically secure)
        var otp = RandomNumberGenerator.GetInt32(100_000, 1_000_000).ToString();

        // 2. Invalidate all previous OTPs for this user
        await _context.PasswordResetOtps
            .Where(x => x.UserId == user.Id && !x.IsUsed)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsUsed, true));

        // 3. Hash OTP using PasswordHasher (salted + secure)
        var hasher = new PasswordHasher<ApplicationUser>();
        var codeHash = hasher.HashPassword(user, otp);

        // 4. Save new OTP record
        var otpEntity = new PasswordResetOtp
        {
            UserId = user.Id,
            CodeHash = codeHash,
            ExpiresOnUtc = DateTime.UtcNow.AddMinutes(10)
        };

        _context.PasswordResetOtps.Add(otpEntity);
        await _context.SaveChangesAsync();

        // 5. Log for dev only — remove in production ⚠
        _logger.LogInformation("OTP for {Email}: {OTP}", email, otp);

        // 6. Send email
        await SendResetPasswordEmail(user, otp);

        return Result.Success();
    }


    public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null || !user.EmailConfirmed)
            return Result.Failure(UserErrors.InvalidCode); // misleading hacker

        IdentityResult result;

        try
        {
            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Code));

            result = await _userManager.ResetPasswordAsync(user, code, request.NewPassword);
        }
        catch (FormatException)
        {
            result = IdentityResult.Failed(_userManager.ErrorDescriber.InvalidToken());
        }

        if (result.Succeeded)
            return Result.Success();

        var error = result.Errors.First();

        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status401Unauthorized));
    }



    // Sends email confirmation link with userId and token
    private async Task SendConfirmationEmail(ApplicationUser user, string code)
    {

        // 1. Build the email confirmation link with userId and token

        // var confirmationUrl = $"{origin}/auth/emailConfirmation?userId={user.Id}&code={code}";
        var confirmationUrl = $"{_appSettings.Value.BaseUrl}/auth/confirm-email?userId={user.Id}&code={code}";

        // 2. Generate the email body using HTML template and replace placeholders
        var emailBody = EmailBodyBuilder.GenerateEmailBody("EmailConfirmation",
            templateModel: new Dictionary<string, string>
            {
            { "{{name}}",$"{user.FirstName} {user.LastName}" },
            { "{{action_url}}", confirmationUrl }
            }
        );

        // 3. Send the email with subject and generated body
        await _emailSender.SendEmailAsync(
            user.Email!,
            "Survey Basket: Email Confirmation",
            emailBody
        );
    }

    //add reset password email sender with templated HTML and OTP code
    private async Task SendResetPasswordEmail(ApplicationUser user, string otp)
    {
        var emailBody = EmailBodyBuilder.GenerateEmailBody("ForgetPassword",
            templateModel: new Dictionary<string, string>
            {
            { "{{name}}", user.FirstName },
            { "{{otp}}", otp }
            }
        );

        await _emailSender.SendEmailAsync(
            user.Email!,
            "Survey Basket: Reset Password",
            emailBody
        );
    }

    //private method which used to generate refresh token
    private static string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

}
