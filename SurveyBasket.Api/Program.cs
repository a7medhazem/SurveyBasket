var builder = WebApplication.CreateBuilder(args);

// to use caching
builder.Services.AddOutputCache(options =>
{
    options.AddPolicy("Polls", builder =>
        builder
          .Cache()
          .Expire(TimeSpan.FromSeconds(120))
          .Tag("availableQuestions"));

});

// AddDependencies extension method that registers all application services and dependencies
builder.Services.AddDependencies(builder.Configuration);

// Add Serilog Configurations
builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

app.UseOutputCache();

app.MapControllers();

app.UseExceptionHandler();

app.Run();
