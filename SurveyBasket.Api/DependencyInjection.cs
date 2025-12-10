namespace SurveyBasket.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddDependencies(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddSweggerServices()
            .AddMapsterConf()
            .AddFluentValidaton();      



        services.AddScoped<IPollService, PollService>();

        return services;
    }
    
    public static IServiceCollection AddSweggerServices(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        return services;
    }
   
    public static IServiceCollection AddMapsterConf(this IServiceCollection services)
    {
        var mapingconfig = TypeAdapterConfig.GlobalSettings;
        mapingconfig.Scan(Assembly.GetExecutingAssembly());      
        services.AddSingleton<IMapper>(new Mapper(mapingconfig));

        return services;
    }
  
    public static IServiceCollection AddFluentValidaton(this IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation()
            .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}
 
