namespace SurveyBasket.Api.Persistanse.EntitesConfigurations;

public class VoteAnswerConfigurations : IEntityTypeConfiguration<VoteAnswer>
{
    public void Configure(EntityTypeBuilder<VoteAnswer> builder)
    {
        //each question has one vote and one answer
        builder.HasIndex(x => new { x.VoteId, x.QuestionId }).IsUnique();

    }
}
