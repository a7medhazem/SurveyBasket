namespace SurveyBasket.Api.Entites;

public sealed class Poll : AuditableEntity
{
    public int Id { get; set; }
    public string Tittle { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
    public DateOnly StartsAt { get; set; }
    public DateOnly EndsAt { get; set; }
    public ICollection<Question> Questions { get; set; } = [];
}
