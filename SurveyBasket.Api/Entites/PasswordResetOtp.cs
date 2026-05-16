namespace SurveyBasket.Api.Entites;

public sealed class PasswordResetOtp
{
    public int Id { get; set; }
    public string UserId { get; set; } = default!;
    public ApplicationUser User { get; set; } = default!;
    public string CodeHash { get; set; } = default!;
    public DateTime ExpiresOnUtc { get; set; }
    public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;
    public int Attempts { get; set; } = 0;
    public bool IsUsed { get; set; } = false;
}
