using System.Text.Json.Serialization;
using IncidentAgent.Api.Endpoints;
using IncidentAgent.Api.Health;
using IncidentAgent.Infrastructure;
using IncidentAgent.Infrastructure.Persistence;

const string FrontendCorsPolicy = "Frontends";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails(options =>
    options.CustomizeProblemDetails = context =>
    {
        // Malformed JSON / bad enum values: tell the client what was wrong.
        if (context.Exception is BadHttpRequestException badRequest)
            context.ProblemDetails.Detail = badRequest.InnerException?.Message ?? badRequest.Message;
    });

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// DbContext must be registered before the health check that uses it.
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHealthChecks()
    .AddCheck<PostgresHealthCheck>("postgres", tags: ["ready"]);

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
        policy.WithOrigins("http://localhost:5090", "http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

app.UseExceptionHandler(new ExceptionHandlerOptions
{
    // Request binding failures are client errors (400), not server errors (500).
    StatusCodeSelector = ex => ex is BadHttpRequestException badRequest ? badRequest.StatusCode : StatusCodes.Status500InternalServerError
});
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    await SeedDevelopmentDataAsync(app);
}

app.UseCors(FrontendCorsPolicy);

app.MapGet("/api/hello", () => new HelloResponse("Hello from IncidentAgent API", DateTime.UtcNow))
   .WithName("GetHello");

// Liveness: no dependencies checked.
app.MapHealthChecks("/health", new() { Predicate = _ => false });

// Readiness: checks PostgreSQL.
app.MapHealthChecks("/health/ready", new() { Predicate = check => check.Tags.Contains("ready") });

app.MapIncidentEndpoints();
app.MapReferenceDataEndpoints();

app.Run();

// Inserts missing sample data. A database problem is logged and does not stop the API,
// so /health keeps working and /health/ready reports the failure.
static async Task SeedDevelopmentDataAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    try
    {
        await scope.ServiceProvider.GetRequiredService<DevDataSeeder>().SeedAsync(app.Lifetime.ApplicationStopping);
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Development seed skipped: {Message}", ex.Message);
    }
}

record HelloResponse(string Message, DateTime TimeUtc);
