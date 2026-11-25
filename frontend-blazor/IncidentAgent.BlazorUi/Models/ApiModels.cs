namespace IncidentAgent.BlazorUi.Models;

// Mirrors the API JSON contract (see http://localhost:5080/openapi/v1.json).
// Enums are sent by the API as strings, so they are kept as strings here.

public record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);

public record IncidentListItem(
    Guid Id,
    string IncidentNumber,
    string Title,
    string Priority,
    string Status,
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

public record Lookup(Guid Id, string Name);

public record UserSummary(Guid Id, string FullName);

public record Comment(Guid Id, string Text, Guid? AuthorId, string? AuthorName, DateTime CreatedAtUtc);

public record IncidentDetail(
    Guid Id,
    string IncidentNumber,
    string Title,
    string Description,
    string Priority,
    string Status,
    Lookup Category,
    Lookup Service,
    UserSummary Reporter,
    UserSummary? AssignedTo,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    DateTime? ResolvedAtUtc,
    DateTime? ClosedAtUtc,
    IReadOnlyList<Comment> Comments);

public record IncidentHistoryEntry(
    Guid Id,
    string Action,
    string? OldValue,
    string? NewValue,
    string? Note,
    Guid? ActorUserId,
    string? ActorName,
    DateTime ChangedAtUtc);

public record Category(Guid Id, string Name, string? Description);

public record ApplicationServiceItem(Guid Id, string Name, string? Description, string? OwnerTeam);

public record UserItem(Guid Id, string FullName, string Email);
