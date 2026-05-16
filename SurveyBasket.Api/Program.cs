var builder = WebApplication.CreateBuilder(args);

// AddDependencies extension method that registers all application services and dependencies
builder.Services.AddDependencies(builder.Configuration);

// Add Serilog Configurations
builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));

var app = builder.Build();


app.UseSwagger();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "SurveyBasket API v1");
    options.RoutePrefix = "swagger";
});


app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();


app.MapControllers();

app.UseExceptionHandler();

app.Run();
