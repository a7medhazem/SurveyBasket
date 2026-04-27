namespace SurveyBasket.Api.Persistanse.EntitesConfigurations;

public class PasswordResetOtpConfiguration : IEntityTypeConfiguration<PasswordResetOtp>
{
    public void Configure(EntityTypeBuilder<PasswordResetOtp> builder)
    {
        builder.Property(x => x.CodeHash)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasOne(x => x.User)
            .WithMany()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.UserId, x.IsUsed, x.ExpiresOnUtc });
    }
}