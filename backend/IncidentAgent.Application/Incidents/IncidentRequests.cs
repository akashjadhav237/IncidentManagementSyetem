using IncidentAgent.Domain.Enums;

namespace IncidentAgent.Application.Incidents;

// Fields are nullable so that missing values in the JSON body can be reported as validation errors.

public record CreateIncidentRequest(
    string? Title,
    string? Description,
    Priority? Priority,
    Guid? CategoryId,
    Guid? ServiceId,
    Guid? ReporterId,
    Guid? ActorUserId = null);

public record UpdateIncidentRequest(
    string? Title,
    string? Description,
    Priority? Priority,
    Guid? CategoryId,
    Guid? ServiceId,
    Guid? ActorUserId = null);

public record AssignIncidentRequest(Guid? AssigneeId, Guid? ActorUserId = null);

public record ChangeStatusRequest(IncidentStatus? NewStatus, Guid? ActorUserId = null, string? Note = null);

public record AddCommentRequest(string? Text, Guid? ActorUserId = null);

public record IncidentListQuery(
    int? Page,
    int? PageSize,
    IncidentStatus? Status,
    Priority? Priority,
    Guid? CategoryId,
    Guid? ServiceId,
    Guid? AssignedToId,
    string? Search);
