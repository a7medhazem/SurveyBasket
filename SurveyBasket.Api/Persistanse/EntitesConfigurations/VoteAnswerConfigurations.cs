namespace SurveyBasket.Api.Persistanse.EntitesConfigurations;

public class VoteAnswerConfigurations : IEntityTypeConfiguration<VoteAnswer>
{
    public void Configure(EntityTypeBuilder<VoteAnswer> builder)
    {
        builder.HasIndex(x => new { x.VoteId, x.QuestionId }).IsUnique();

    }
}
