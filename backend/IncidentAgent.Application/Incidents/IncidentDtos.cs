using IncidentAgent.Domain.Enums;

namespace IncidentAgent.Application.Incidents;

public record UserSummaryDto(Guid Id, string FullName);

public record LookupDto(Guid Id, string Name);

public record IncidentListItemDto(
    Guid Id,
    string IncidentNumber,
    string Title,
    Priority Priority,
    IncidentStatus Status,
    Guid CategoryId,
    string CategoryName,
    Guid ServiceId,
    string ServiceName,
    Guid ReporterId,
    string ReporterName,
    Guid? AssignedToId,
    string? AssignedToName,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public record CommentDto(Guid Id, string Text, Guid? AuthorId, string? AuthorName, DateTime CreatedAtUtc);

public record IncidentDetailsDto(
    Guid Id,
    string IncidentNumber,
    string Title,
    string Description,
    Priority Priority,
    IncidentStatus Status,
    LookupDto Category,
    LookupDto Service,
    UserSummaryDto Reporter,
    UserSummaryDto? AssignedTo,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    DateTime? ResolvedAtUtc,
    DateTime? ClosedAtUtc,
    IReadOnlyList<CommentDto> Comments);

public record IncidentHistoryDto(
    Guid Id,
    HistoryAction Action,
    string? OldValue,
    string? NewValue,
    string? Note,
    Guid? ActorUserId,
    string? ActorName,
    DateTime ChangedAtUtc);
