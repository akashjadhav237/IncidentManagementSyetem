using IncidentAgent.Application.ReferenceData;

namespace IncidentAgent.Api.Endpoints;

public static class ReferenceDataEndpoints
{
    public static IEndpointRouteBuilder MapReferenceDataEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api").WithTags("Reference data");

        group.MapGet("/categories", async (IReferenceDataService service, CancellationToken ct) =>
                Results.Ok(await service.GetCategoriesAsync(ct)))
            .WithName("ListCategories")
            .Produces<IReadOnlyList<CategoryDto>>();

        group.MapGet("/services", async (IReferenceDataService service, CancellationToken ct) =>
                Results.Ok(await service.GetServicesAsync(ct)))
            .WithName("ListServices")
            .Produces<IReadOnlyList<ServiceDto>>();

        group.MapGet("/users", async (IReferenceDataService service, CancellationToken ct) =>
                Results.Ok(await service.GetUsersAsync(ct)))
            .WithName("ListUsers")
            .Produces<IReadOnlyList<UserDto>>();

        return app;
    }
}
