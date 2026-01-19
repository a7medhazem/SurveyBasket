
using SurveyBasket.Api.Authentication;
using System.Security.Cryptography;

namespace SurveyBasket.Api.Services;

public class AuthService(UserManager<ApplicationUser> userManager, IJwtProvider jwtProvider) : IAuthService
{
    private readonly UserManager<ApplicationUser> _UserManager = userManager;//to use identity methods
    private readonly IJwtProvider _JwtProvider = jwtProvider;//to call GenerateToken method
    private readonly int _RefreshTokenExpiryDays = 14;

    public async Task<AuthResponse?> GetTokenAsync(string email, string password, CancellationToken cancellationToken)
    {
        //check user
        var user = await _UserManager.FindByEmailAsync(email);

        if (user is null)
            return null;

        //check password 
        var isValidPassword = await _UserManager.CheckPasswordAsync(user, password);

        if (!isValidPassword)
            return null;

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



        //return new auth response
        return new AuthResponse(user.Id, user.Email!, user.FirstName, user.LastName, token, expiresIn, refreshToken, refreshTokenExpiration);

    }
    private static string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }
}
