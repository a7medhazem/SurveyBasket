using System.Globalization;

namespace SurveyBasket.Api.Contracts.Users;

public record UpdateProfileRequest(
    string FirstName,
    string LastName
);