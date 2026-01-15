namespace SurveyBasket.Api.Persistanse.EntitesConfigurations;

public class PollConfiguration : IEntityTypeConfiguration<Poll>
{
    public void Configure(EntityTypeBuilder<Poll> builder)
    {
        builder.HasIndex(x => x.Tittle).IsUnique();
        builder.Property(x => x.Tittle).HasMaxLength(100);
        builder.Property(x => x.Summary).HasMaxLength(1500);
    }
}
