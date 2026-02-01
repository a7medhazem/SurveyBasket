using SurveyBasket.Api.Authentication;
using System.Security.Cryptography;

namespace SurveyBasket.Api.Services;

public class AuthService(UserManager<ApplicationUser> userManager, IJwtProvider jwtProvider) : IAuthService
{
    private readonly UserManager<ApplicationUser> _UserManager = userManager;//to use identity methods
    private readonly IJwtProvider _JwtProvider = jwtProvider;//to call GenerateToken method
    private readonly int _RefreshTokenExpiryDays = 14;

    public async Task<Result<AuthResponse>> GetTokenAsync(string email, string password, CancellationToken cancellationToken)
    {
        //check user
        var user = await _UserManager.FindByEmailAsync(email);

        if (user is null)
            return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);

        //check password 
        var isValidPassword = await _UserManager.CheckPasswordAsync(user, password);

        if (!isValidPassword)
            return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);


        //generate JWT token
        (string token, int expiresIn) = _JwtProvider.GenerateToken(user);

        //generate Refresh Token 
        var refreshToken = GenerateRefreshToken();
        var refreshTokenExpiration = DateTime.UtcNow.AddDays(_RefreshTokenExpiryDays);

        user.RefreshTokens.Add(
            new RefreshToken()
            {
                Token = refreshToken,
                ExpiresOn = refreshTokenExpiration

            });
        await _UserManager.UpdateAsync(user);

        var response = new AuthResponse(user.Id, user.Email!, user.FirstName, user.LastName, token, expiresIn, refreshToken, refreshTokenExpiration);

        //return new auth response
        return Result.Success(response);

    }


    public async Task<Result<AuthResponse>> GetRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default)
    {
        var userId = _JwtProvider.ValidateToken(token);//return user id or null

        if (userId is null)
            return Result.Failure<AuthResponse>(UserErrors.InvalidJwtToken);


        var user = await _UserManager.FindByIdAsync(userId);

        if (user is null)
            return Result.Failure<AuthResponse>(UserErrors.InvalidJwtToken);


        var UserRefreshToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken && x.IsActive);

        if (UserRefreshToken is null)
            return Result.Failure<AuthResponse>(UserErrors.InvalidJwtToken);


        UserRefreshToken.RevokedOn = DateTime.UtcNow;

        // After validating the access and refresh tokens for the current user,
        // generate new access and refresh tokens.

        //generate JWT token
        (string newToken, int expiresIn) = _JwtProvider.GenerateToken(user);

        //generate Refresh Token 
        var newRefreshToken = GenerateRefreshToken();
        var refreshTokenExpiration = DateTime.UtcNow.AddDays(_RefreshTokenExpiryDays);

        user.RefreshTokens.Add(
            new RefreshToken()
            {
                Token = newRefreshToken,
                ExpiresOn = refreshTokenExpiration
            });
        await _UserManager.UpdateAsync(user);

        //return new auth response
        var response = new AuthResponse(user.Id, user.Email!, user.FirstName, user.LastName, newToken, expiresIn, newRefreshToken, refreshTokenExpiration);

        return Result.Success(response);

    }


    public async Task<Result> RevokeRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default)
    {
        var userId = _JwtProvider.ValidateToken(token);//return user id or null

        if (userId is null)
            return Result.Failure(UserErrors.InvalidJwtToken);


        var user = await _UserManager.FindByIdAsync(userId);

        if (user is null)
            return Result.Failure(UserErrors.InvalidJwtToken);

        var UserRefreshToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken && x.IsActive);

        if (UserRefreshToken is null)
            return Result.Failure(UserErrors.InvalidRefreshToken);


        UserRefreshToken.RevokedOn = DateTime.UtcNow;

        await _UserManager.UpdateAsync(user);

        return Result.Success();//refresh token is revoked successfuly

    }



    //private method which used to generate refresh token
    private static string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

}
