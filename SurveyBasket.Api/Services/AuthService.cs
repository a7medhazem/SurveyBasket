
using SurveyBasket.Api.Authentication;

namespace SurveyBasket.Api.Services;

public class AuthService(UserManager<ApplicationUser> userManager, IJwtProvider jwtProvider) : IAuthService
{
    private readonly UserManager<ApplicationUser> _UserManager = userManager;//to use identity methods
    private readonly IJwtProvider _JwtProvider = jwtProvider;//to call GenerateToken method

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

        //return new auth response
        return new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, token, expiresIn);

    }
}
