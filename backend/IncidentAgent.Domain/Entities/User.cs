namespace IncidentAgent.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public ICollection<Incident> ReportedIncidents { get; set; } = new List<Incident>();
    public ICollection<Incident> AssignedIncidents { get; set; } = new List<Incident>();
}
