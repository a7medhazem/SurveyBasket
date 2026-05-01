namespace SurveyBasket.Api.Services;

public class AuthService(UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ILogger<AuthService> logger,
    IJwtProvider jwtProvider,
    IEmailSender emailSender,
    IOptions<AppSettings> appSettings,
    ApplicationDbContext context,
    IPasswordHasher<ApplicationUser> passwordHasher) : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;//to use identity methods
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;// Handles the sign-in process efficiently
    private readonly ILogger<AuthService> _logger = logger;
    private readonly IJwtProvider _jwtProvider = jwtProvider;//to call GenerateToken method
    private readonly IEmailSender _emailSender = emailSender;
    private readonly IOptions<AppSettings> _appSettings = appSettings;
    private readonly ApplicationDbContext _context = context;
    private readonly IPasswordHasher<ApplicationUser> _passwordHasher = passwordHasher;

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

    public async Task<Result> SendResetPasswordCodeAsync(string email, CancellationToken cancellationToken)
    {
        if (await _userManager.FindByEmailAsync(email) is not { } user)
            return Result.Success();//misleading

        if (!user.EmailConfirmed)
            return Result.Failure(UserErrors.EmailNotConfirmed);


        // 1. Generate OTP (6 digits - cryptographically secure)
        var otp = RandomNumberGenerator.GetInt32(100_000, 1_000_000).ToString();

        // 2. Invalidate all previous OTPs for this user
        await _context.PasswordResetOtps
            .Where(x => x.UserId == user.Id && !x.IsUsed)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsUsed, true), cancellationToken: cancellationToken);

        // 3. Hash OTP using PasswordHasher (salted + secure)
        var codeHash = _passwordHasher.HashPassword(user, otp);

        // 4. Save new OTP record
        var otpEntity = new PasswordResetOtp
        {
            UserId = user.Id,
            CodeHash = codeHash,
            ExpiresOnUtc = DateTime.UtcNow.AddMinutes(5)//code expire after 5 minitues
        };

        _context.PasswordResetOtps.Add(otpEntity);
        await _context.SaveChangesAsync(cancellationToken);

        // 5. Log for dev only
        _logger.LogInformation("OTP for {Email}: {OTP}", email, otp);

        // 6. Send email
        await SendResetPasswordEmail(user, otp);

        return Result.Success();
    }


    public async Task<Result<VerifyOtpResponse>> VerifyResetPasswordOtpAsync(VerifyOtpRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null || !user.EmailConfirmed)
            return Result.Failure<VerifyOtpResponse>(UserErrors.InvalidCode);

        // 1. Retrieve the latest valid OTP (not used and not expired)
        var otp = await _context.PasswordResetOtps
            .Where(x =>
                x.UserId == user.Id &&
                !x.IsUsed &&
                x.ExpiresOnUtc > DateTime.UtcNow)
            .OrderByDescending(x => x.CreatedOnUtc)
            .FirstOrDefaultAsync(cancellationToken);

        // 2. If no valid OTP found, check if there was an expired one (better UX for client)
        if (otp is null)
        {
            var hasExpired = await _context.PasswordResetOtps
                .AnyAsync(x => x.UserId == user.Id && !x.IsUsed, cancellationToken);

            return hasExpired
                ? Result.Failure<VerifyOtpResponse>(UserErrors.ExpiredCode)
                : Result.Failure<VerifyOtpResponse>(UserErrors.InvalidCode);
        }

        // 3. Check if maximum number of attempts has been reached
        if (otp.Attempts >= 5)
        {
            otp.IsUsed = true;
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Failure<VerifyOtpResponse>(UserErrors.TooManyAttempts);
        }

        // 4. Verify the provided OTP against the stored hash
        var verifyResult = _passwordHasher.VerifyHashedPassword(user, otp.CodeHash, request.OtpCode);

        if (verifyResult == PasswordVerificationResult.Failed)
        {
            otp.Attempts++;
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Failure<VerifyOtpResponse>(UserErrors.InvalidCode);
        }

        // 5. Mark the OTP as used after successful verification
        otp.IsUsed = true;
        await _context.SaveChangesAsync(cancellationToken);

        // 6. Generate and encode password reset token
        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(resetToken));

        return Result.Success(new VerifyOtpResponse(encodedToken));
    }


    public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null || !user.EmailConfirmed)
            return Result.Failure(UserErrors.InvalidCredentials);

        // Decode the reset token
        string decodedToken;
        try
        {
            decodedToken = Encoding.UTF8.GetString(
                WebEncoders.Base64UrlDecode(request.ResetToken));
        }
        catch
        {
            return Result.Failure(UserErrors.InvalidResetToken);
        }

        // Reset password via Identity
        var result = await _userManager.ResetPasswordAsync(user, decodedToken, request.NewPassword);

        if (!result.Succeeded)
        {
            var error = result.Errors.First();
            return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
        }

        return Result.Success();
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
               { "{{otp}}", otp },
               { "{{expiry_minutes}}", "5" }
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
