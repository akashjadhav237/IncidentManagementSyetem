using IncidentAgent.Domain.Enums;

namespace IncidentAgent.Domain.Entities;

public class IncidentHistory
{
    public Guid Id { get; set; }

    public Guid IncidentId { get; set; }
    public Incident Incident { get; set; } = null!;

    public HistoryAction Action { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? Note { get; set; }

    public Guid? ActorUserId { get; set; }
    public User? ActorUser { get; set; }

    public DateTime ChangedAtUtc { get; set; }
}
