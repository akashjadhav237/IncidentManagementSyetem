using IncidentAgent.Application.Common;
using IncidentAgent.Application.Incidents;

namespace IncidentAgent.Api.Endpoints;

public static class IncidentEndpoints
{
    public static IEndpointRouteBuilder MapIncidentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/incidents").WithTags("Incidents");

        group.MapPost("/", async (CreateIncidentRequest request, IIncidentService service, CancellationToken ct) =>
                (await service.CreateAsync(request, ct)).ToHttpResult(dto => Results.Created($"/api/incidents/{dto.Id}", dto)))
            .WithName("CreateIncident")
            .Produces<IncidentDetailsDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapGet("/", async ([AsParameters] IncidentListQuery query, IIncidentService service, CancellationToken ct) =>
                (await service.ListAsync(query, ct)).ToHttpResult(Results.Ok))
            .WithName("ListIncidents")
            .Produces<PagedResult<IncidentListItemDto>>()
            .ProducesValidationProblem();

        group.MapGet("/{id:guid}", async (Guid id, IIncidentService service, CancellationToken ct) =>
                (await service.GetByIdAsync(id, ct)).ToHttpResult(Results.Ok))
            .WithName("GetIncident")
            .Produces<IncidentDetailsDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}", async (Guid id, UpdateIncidentRequest request, IIncidentService service, CancellationToken ct) =>
                (await service.UpdateAsync(id, request, ct)).ToHttpResult(Results.Ok))
            .WithName("UpdateIncident")
            .Produces<IncidentDetailsDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/assign", async (Guid id, AssignIncidentRequest request, IIncidentService service, CancellationToken ct) =>
                (await service.AssignAsync(id, request, ct)).ToHttpResult(Results.Ok))
            .WithName("AssignIncident")
            .Produces<IncidentDetailsDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/status", async (Guid id, ChangeStatusRequest request, IIncidentService service, CancellationToken ct) =>
                (await service.ChangeStatusAsync(id, request, ct)).ToHttpResult(Results.Ok))
            .WithName("ChangeIncidentStatus")
            .Produces<IncidentDetailsDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/comments", async (Guid id, AddCommentRequest request, IIncidentService service, CancellationToken ct) =>
                (await service.AddCommentAsync(id, request, ct)).ToHttpResult(dto => Results.Created($"/api/incidents/{id}", dto)))
            .WithName("AddIncidentComment")
            .Produces<CommentDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapGet("/{id:guid}/history", async (Guid id, IIncidentService service, CancellationToken ct) =>
                (await service.GetHistoryAsync(id, ct)).ToHttpResult(Results.Ok))
            .WithName("GetIncidentHistory")
            .Produces<IReadOnlyList<IncidentHistoryDto>>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
