using IncidentAgent.Application.Incidents;
using IncidentAgent.Domain.Entities;
using IncidentAgent.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IncidentAgent.Infrastructure.Persistence;

/// <summary>
/// Development-only sample data. Idempotent: every row is inserted only if it is missing.
/// GUIDs are fixed so they never change between runs.
/// </summary>
public class DevDataSeeder(ApplicationDbContext db, IIncidentService incidents, ILogger<DevDataSeeder> logger)
{
    public static class Ids
    {
        public static readonly Guid CategoryApplicationError = Guid.Parse("3f1a7c52-9b1e-4d6a-8c3e-1a2b3c4d5e01");
        public static readonly Guid CategoryInfrastructure = Guid.Parse("3f1a7c52-9b1e-4d6a-8c3e-1a2b3c4d5e02");
        public static readonly Guid CategoryNetwork = Guid.Parse("3f1a7c52-9b1e-4d6a-8c3e-1a2b3c4d5e03");
        public static readonly Guid CategorySecurity = Guid.Parse("3f1a7c52-9b1e-4d6a-8c3e-1a2b3c4d5e04");

        public static readonly Guid ServicePaymentApi = Guid.Parse("7d2e9a10-4c5b-4f8a-9e6d-2b3c4d5e6f02");
        public static readonly Guid ServiceCustomerPortal = Guid.Parse("7d2e9a10-4c5b-4f8a-9e6d-2b3c4d5e6f03");
        public static readonly Guid ServiceOrderService = Guid.Parse("7d2e9a10-4c5b-4f8a-9e6d-2b3c4d5e6f04");
        public static readonly Guid ServiceNotificationService = Guid.Parse("7d2e9a10-4c5b-4f8a-9e6d-2b3c4d5e6f05");

        public static readonly Guid UserAdmin = Guid.Parse("a9c4e8b3-1d7f-4a2c-b5e9-3c4d5e6f7a03");
        public static readonly Guid UserPriya = Guid.Parse("a9c4e8b3-1d7f-4a2c-b5e9-3c4d5e6f7a04");
        public static readonly Guid UserDaniel = Guid.Parse("a9c4e8b3-1d7f-4a2c-b5e9-3c4d5e6f7a05");

        public static readonly Guid SlaP1 = Guid.Parse("c1d2e3f4-0a1b-4c2d-9e3f-4a5b6c7d8e01");
        public static readonly Guid SlaP2 = Guid.Parse("c1d2e3f4-0a1b-4c2d-9e3f-4a5b6c7d8e02");
        public static readonly Guid SlaP3 = Guid.Parse("c1d2e3f4-0a1b-4c2d-9e3f-4a5b6c7d8e03");
        public static readonly Guid SlaP4 = Guid.Parse("c1d2e3f4-0a1b-4c2d-9e3f-4a5b6c7d8e04");
    }

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        await AddIfMissingAsync(db.IncidentCategories, [
            new IncidentCategory { Id = Ids.CategoryApplicationError, Name = "Application Error", Description = "Bugs, crashes and unexpected errors in applications.", CreatedAtUtc = now },
            new IncidentCategory { Id = Ids.CategoryInfrastructure, Name = "Infrastructure", Description = "Servers, storage, databases and cloud resources.", CreatedAtUtc = now },
            new IncidentCategory { Id = Ids.CategoryNetwork, Name = "Network", Description = "Connectivity, DNS, load balancers and latency.", CreatedAtUtc = now },
            new IncidentCategory { Id = Ids.CategorySecurity, Name = "Security", Description = "Suspicious activity, access and certificate issues.", CreatedAtUtc = now }
        ], c => c.Id, cancellationToken);

        await AddIfMissingAsync(db.ApplicationServices, [
            new ApplicationService { Id = Ids.ServicePaymentApi, Name = "Payment API", Description = "Card and wallet payment processing.", OwnerTeam = "Payments", CreatedAtUtc = now },
            new ApplicationService { Id = Ids.ServiceCustomerPortal, Name = "Customer Portal", Description = "Customer-facing web portal.", OwnerTeam = "Web", CreatedAtUtc = now },
            new ApplicationService { Id = Ids.ServiceOrderService, Name = "Order Service", Description = "Order creation and fulfilment.", OwnerTeam = "Commerce", CreatedAtUtc = now },
            new ApplicationService { Id = Ids.ServiceNotificationService, Name = "Notification Service", Description = "Email, SMS and push notifications.", OwnerTeam = "Platform", CreatedAtUtc = now }
        ], s => s.Id, cancellationToken);

        await AddIfMissingAsync(db.Users, [
            new User { Id = Ids.UserAdmin, FullName = "Admin User", Email = "admin@incidentagent.local", CreatedAtUtc = now },
            new User { Id = Ids.UserPriya, FullName = "Priya Sharma", Email = "priya.sharma@incidentagent.local", CreatedAtUtc = now },
            new User { Id = Ids.UserDaniel, FullName = "Daniel Okafor", Email = "daniel.okafor@incidentagent.local", CreatedAtUtc = now }
        ], u => u.Id, cancellationToken);

        await AddIfMissingAsync(db.SlaPolicies, [
            new SlaPolicy { Id = Ids.SlaP1, Priority = Priority.P1Critical, ResponseTimeMinutes = 15, ResolutionTimeMinutes = 240, CreatedAtUtc = now },
            new SlaPolicy { Id = Ids.SlaP2, Priority = Priority.P2High, ResponseTimeMinutes = 60, ResolutionTimeMinutes = 480, CreatedAtUtc = now },
            new SlaPolicy { Id = Ids.SlaP3, Priority = Priority.P3Medium, ResponseTimeMinutes = 240, ResolutionTimeMinutes = 1440, CreatedAtUtc = now },
            new SlaPolicy { Id = Ids.SlaP4, Priority = Priority.P4Low, ResponseTimeMinutes = 480, ResolutionTimeMinutes = 4320, CreatedAtUtc = now }
        ], p => p.Id, cancellationToken);

        await SeedIncidentsAsync(cancellationToken);
    }

    private async Task AddIfMissingAsync<T>(DbSet<T> set, T[] rows, Func<T, Guid> getId, CancellationToken cancellationToken)
        where T : class
    {
        var ids = rows.Select(getId).ToList();
        var existing = await set.AsNoTracking()
            .Where(e => ids.Contains(EF.Property<Guid>(e, "Id")))
            .Select(e => EF.Property<Guid>(e, "Id"))
            .ToListAsync(cancellationToken);

        var missing = rows.Where(r => !existing.Contains(getId(r))).ToList();
        if (missing.Count == 0) return;

        set.AddRange(missing);
        await db.SaveChangesAsync(cancellationToken);
        db.ChangeTracker.Clear();
        logger.LogInformation("Seeded {Count} {Entity} rows.", missing.Count, typeof(T).Name);
    }

    private record SampleIncident(string Title, string Description, Priority Priority, Guid CategoryId, Guid ServiceId,
        Guid ReporterId, Guid? AssigneeId, IncidentStatus[] StatusPath);

    private async Task SeedIncidentsAsync(CancellationToken cancellationToken)
    {
        IncidentStatus[] investigating = [IncidentStatus.Investigating];
        IncidentStatus[] pending = [IncidentStatus.Investigating, IncidentStatus.Pending];
        IncidentStatus[] resolved = [IncidentStatus.Investigating, IncidentStatus.Resolved];
        IncidentStatus[] closed = [IncidentStatus.Investigating, IncidentStatus.Resolved, IncidentStatus.Closed];

        SampleIncident[] samples =
        [
            new("Payment API returns 500 on checkout", "Customers receive HTTP 500 when paying by card at checkout.", Priority.P1Critical, Ids.CategoryApplicationError, Ids.ServicePaymentApi, Ids.UserAdmin, Ids.UserPriya, investigating),
            new("Customer Portal login page slow", "Login page takes more than 10 seconds to load during peak hours.", Priority.P2High, Ids.CategoryApplicationError, Ids.ServiceCustomerPortal, Ids.UserDaniel, null, []),
            new("Order Service database CPU at 95%", "Primary database for Order Service shows sustained high CPU.", Priority.P1Critical, Ids.CategoryInfrastructure, Ids.ServiceOrderService, Ids.UserAdmin, Ids.UserDaniel, []),
            new("Notification emails delayed", "Order confirmation emails are delayed by up to 30 minutes.", Priority.P3Medium, Ids.CategoryApplicationError, Ids.ServiceNotificationService, Ids.UserPriya, Ids.UserDaniel, pending),
            new("Intermittent DNS failures for portal", "Some users report the portal domain cannot be resolved.", Priority.P2High, Ids.CategoryNetwork, Ids.ServiceCustomerPortal, Ids.UserDaniel, Ids.UserPriya, resolved),
            new("Expired TLS certificate on payment callback", "Payment provider callbacks fail because the certificate expired.", Priority.P1Critical, Ids.CategorySecurity, Ids.ServicePaymentApi, Ids.UserPriya, Ids.UserAdmin, closed),
            new("Typo on order confirmation page", "The confirmation page says 'Oder' instead of 'Order'.", Priority.P4Low, Ids.CategoryApplicationError, Ids.ServiceOrderService, Ids.UserAdmin, null, []),
            new("Push notifications not sent on Android", "Android users stopped receiving push notifications after the last release.", Priority.P2High, Ids.CategoryApplicationError, Ids.ServiceNotificationService, Ids.UserDaniel, Ids.UserAdmin, investigating),
            new("Load balancer health check flapping", "Order Service nodes are repeatedly removed and re-added to the pool.", Priority.P3Medium, Ids.CategoryNetwork, Ids.ServiceOrderService, Ids.UserPriya, Ids.UserDaniel, resolved),
            new("Suspicious login attempts on portal", "Many failed login attempts from a small IP range.", Priority.P3Medium, Ids.CategorySecurity, Ids.ServiceCustomerPortal, Ids.UserAdmin, null, [])
        ];

        var existingTitles = await db.Incidents.AsNoTracking().Select(i => i.Title).ToListAsync(cancellationToken);
        var created = 0;

        foreach (var sample in samples.Where(s => !existingTitles.Contains(s.Title)))
        {
            var result = await incidents.CreateAsync(
                new CreateIncidentRequest(sample.Title, sample.Description, sample.Priority, sample.CategoryId, sample.ServiceId, sample.ReporterId),
                cancellationToken);
            if (!result.IsSuccess)
            {
                logger.LogWarning("Seed incident '{Title}' failed: {Error}", sample.Title, result.Error);
                continue;
            }

            var id = result.Value!.Id;
            if (sample.AssigneeId is not null)
                await incidents.AssignAsync(id, new AssignIncidentRequest(sample.AssigneeId, Ids.UserAdmin), cancellationToken);

            foreach (var status in sample.StatusPath)
                await incidents.ChangeStatusAsync(id, new ChangeStatusRequest(status, Ids.UserAdmin, "Seed data"), cancellationToken);

            if (sample.StatusPath.Length > 0)
                await incidents.AddCommentAsync(id, new AddCommentRequest("Looking into this now.", sample.AssigneeId ?? Ids.UserAdmin), cancellationToken);

            created++;
        }

        if (created > 0)
            logger.LogInformation("Seeded {Count} sample incidents.", created);
    }
}
