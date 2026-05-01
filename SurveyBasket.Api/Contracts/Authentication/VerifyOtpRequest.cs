namespace SurveyBasket.Api.Contracts.Authentication;


public record VerifyOtpRequest(
    string Email,
    string OtpCode
);