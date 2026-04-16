using Microsoft.AspNetCore.WebUtilities;
using System.Net;
using System.Text;

namespace SurveyBasket.Api.Services;

public class AuthService(UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ILogger<AuthService> logger,
    IJwtProvider jwtProvider) : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;//to use identity methods
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;// Handles the sign-in process efficiently
    private readonly ILogger<AuthService> _logger = logger;
    private readonly IJwtProvider _jwtProvider = jwtProvider;//to call GenerateToken method
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

            // TODO: Send email with confirmation link

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

        // TODO: Send email with confirmation link

        return Result.Success();
    }

    //private method which used to generate refresh token
    private static string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

}
