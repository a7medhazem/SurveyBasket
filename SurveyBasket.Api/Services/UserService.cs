using System.Threading;

namespace SurveyBasket.Api.Services;

public class UserService(UserManager<ApplicationUser> userManager) : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public async Task<Result<UserProfileResponse>> GetProfileAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users
            .Where(x => x.Id == userId)
            .ProjectToType<UserProfileResponse>()
            .SingleAsync(cancellationToken);

        return Result.Success(user);
    }

    public async Task<Result> UpdateProfileAsync(string userId, UpdateProfileRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId);

        // Apply request data to the existing tracked user instance
        user = request.Adapt(user);


        await _userManager.Users
            .Where(x => x.Id == userId)
            .ExecuteUpdateAsync(setter =>
                setter
                  .SetProperty(x => x.FirstName, request.FirstName)
                  .SetProperty(x => x.LastName, request.LastName)
            );

        return Result.Success();
    }
    public async Task<Result> ChangePasswordAsync(string userId, ChangePasswordRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId);


        var result = await _userManager.ChangePasswordAsync(
            user!,
            request.CurrentPassword,
            request.NewPassword
        );

        if (result.Succeeded)
        {
            return Result.Success();
        }

        var error = result.Errors.First();

        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }
}
