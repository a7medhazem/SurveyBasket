using SurveyBasket.Api.Contracts.Users;
namespace SurveyBasket.Api.Services;

public interface IUserService
{
    Task<Result<UserProfileResponse>> GetProfileAsync(string userId);

}
