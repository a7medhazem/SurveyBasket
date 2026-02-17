var builder = WebApplication.CreateBuilder(args);
//
// AddDependencies extension method that registers all application services and dependencies
builder.Services.AddDependencies(builder.Configuration);

var config = builder.Configuration;


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();


app.MapControllers();

app.UseExceptionHandler();

app.Run();
