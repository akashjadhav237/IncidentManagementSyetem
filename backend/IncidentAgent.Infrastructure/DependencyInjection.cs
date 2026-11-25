using IncidentAgent.Application.Incidents;
using IncidentAgent.Application.ReferenceData;
using IncidentAgent.Infrastructure.Persistence;
using IncidentAgent.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IncidentAgent.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException(
                "Connection string 'Postgres' is missing. Set it with: dotnet user-secrets set \"ConnectionStrings:Postgres\" \"...\" --project backend/IncidentAgent.Api");

        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IIncidentService, IncidentService>();
        services.AddScoped<IReferenceDataService, ReferenceDataService>();
        services.AddScoped<DevDataSeeder>();

        return services;
    }
}
