namespace SurveyBasket.Api.Persistanse.EntitesConfigurations;

public class VoteConfigurations : IEntityTypeConfiguration<Vote>
{
    public void Configure(EntityTypeBuilder<Vote> builder)
    {
        builder.HasIndex(x => new { x.PollId, x.UserId }).IsUnique();
    }
}