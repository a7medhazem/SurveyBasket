using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SurveyBasket.Api.Authentication;
using System.Text;

namespace SurveyBasket.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddDependencies(this IServiceCollection services, IConfiguration Configuration)
    {
        services.AddControllers();

        var allowedOrigins = Configuration.GetSection("AllowedOrigins").Get<string[]>();

        services.AddCors(Options =>

          Options.AddDefaultPolicy(builder =>

              builder
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithOrigins(allowedOrigins!)
          )

        );

        services.AddSweggerServicesConfig()
            .AddMapsterConfig()
            .AddFluentValidatonConfig();

        services.AddAuthConfig(Configuration);


        //add ConnectionString and register ApplicationDbContext
        var connectionString = Configuration.GetConnectionString("DefaultConnection") ??
             throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(options
            => options.UseSqlServer(connectionString));


        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPollService, PollService>();
        services.AddScoped<IQuestionService, QuestionService>();
        services.AddScoped<IVoteService, VoteService>();

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }

    private static IServiceCollection AddSweggerServicesConfig(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        return services;
    }

    private static IServiceCollection AddMapsterConfig(this IServiceCollection services)
    {
        var mapingconfig = TypeAdapterConfig.GlobalSettings;
        mapingconfig.Scan(Assembly.GetExecutingAssembly());
        services.AddSingleton<IMapper>(new Mapper(mapingconfig));

        return services;
    }

    private static IServiceCollection AddFluentValidatonConfig(this IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation()
            .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
    private static IServiceCollection AddAuthConfig(this IServiceCollection services, IConfiguration configuration)
    {
        // Must inform the program that Identity will be used
        services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddSingleton<IJwtProvider, JwtProvider>();



        // services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services
            .AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();


        var jwtSettings = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>();


        //jwt configurations
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(o =>
        {
            o.SaveToken = true;
            o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings?.Key!)),
                ValidIssuer = jwtSettings?.Issuer,
                ValidAudience = jwtSettings?.Audience
            };
        });


        return services;
    }
}

