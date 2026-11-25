using IncidentAgent.Domain.Enums;

namespace IncidentAgent.Domain.Entities;

public class SlaPolicy
{
    public Guid Id { get; set; }
    public Priority Priority { get; set; }
    public int ResponseTimeMinutes { get; set; }
    public int ResolutionTimeMinutes { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
