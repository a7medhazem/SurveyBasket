namespace SurveyBasket.Api.Errors;

public static class UserErrors
{
    public static readonly Error InvalidCredentials =
        new("User.InvalidCredentials", "Invalid email / password", StatusCodes.Status401Unauthorized);

    public static readonly Error InvalidJwtToken =
        new("User.InvalidJwtToken", "Invalid Jwt token", StatusCodes.Status401Unauthorized);

    public static readonly Error InvalidRefreshToken =
        new("User.InvalidRefreshToken", "Invalid refresh token", StatusCodes.Status401Unauthorized);

    public static readonly Error DuplicatedEmail =
        new("User.DuplicatedEmail", "Another user with the same email is already exist", StatusCodes.Status409Conflict);

    public static readonly Error EmailNotConfirmed =
        new("User.EmailNotConfirmed", "Email is not confirmed", StatusCodes.Status401Unauthorized);

    public static readonly Error InvalidCode =
       new("User.InvalidCode", "Invalid code", StatusCodes.Status401Unauthorized);

    public static readonly Error DuplicatedConfirmation =
        new("User.DuplicatedConfirmation", "Email already confirmed", StatusCodes.Status400BadRequest);

    public static readonly Error ExpiredCode =
    new("User.ExpiredCode", "OTP code has expired", StatusCodes.Status401Unauthorized);

    public static readonly Error TooManyAttempts =
        new("User.TooManyAttempts", "Too many failed attempts, request a new code", StatusCodes.Status429TooManyRequests);

    public static readonly Error InvalidResetToken =
       new("User.InvalidResetToken", "Invalid or expired reset token", StatusCodes.Status401Unauthorized);


    public static readonly Error CooldownActive =
        new("User.CooldownActive", "Please wait before requesting a new code", StatusCodes.Status429TooManyRequests);
}

