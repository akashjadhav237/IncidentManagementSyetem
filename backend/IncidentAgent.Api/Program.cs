const string FrontendCorsPolicy = "Frontends";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
        policy.WithOrigins("http://localhost:5090", "http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors(FrontendCorsPolicy);

app.MapGet("/api/hello", () => new HelloResponse("Hello from IncidentAgent API", DateTime.UtcNow))
   .WithName("GetHello");

app.MapGet("/health", () => Results.Ok())
   .WithName("Health");

app.Run();

record HelloResponse(string Message, DateTime TimeUtc);
