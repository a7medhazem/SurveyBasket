namespace SurveyBasket.Api.Contracts.Authentication;

public record ResetPasswordRequest(
    string Email,
    string ResetToken,
    string NewPassword,
    string ConfirmPassword
);