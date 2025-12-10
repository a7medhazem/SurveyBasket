namespace SurveyBasket.Api.Mapping;

public class MappingConfigurations : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        //config.NewConfig<Poll, PollResponse>()
        //.Map(d => d.Note, s => s.Description);
    }
}
