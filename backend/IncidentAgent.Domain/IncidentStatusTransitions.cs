using IncidentAgent.Domain.Enums;

namespace IncidentAgent.Domain;

/// <summary>Allowed incident status transitions. Anything not listed is rejected.</summary>
public static class IncidentStatusTransitions
{
    private static readonly Dictionary<IncidentStatus, IncidentStatus[]> Allowed = new()
    {
        [IncidentStatus.Open] = [IncidentStatus.Assigned, IncidentStatus.Investigating, IncidentStatus.Closed],
        [IncidentStatus.Assigned] = [IncidentStatus.Investigating, IncidentStatus.Pending, IncidentStatus.Open],
        [IncidentStatus.Investigating] = [IncidentStatus.Pending, IncidentStatus.Resolved, IncidentStatus.Assigned],
        [IncidentStatus.Pending] = [IncidentStatus.Investigating, IncidentStatus.Resolved],
        [IncidentStatus.Resolved] = [IncidentStatus.Closed, IncidentStatus.Investigating],
        [IncidentStatus.Closed] = []
    };

    public static bool CanTransition(IncidentStatus from, IncidentStatus to) =>
        Allowed.TryGetValue(from, out var targets) && targets.Contains(to);

    public static IReadOnlyList<IncidentStatus> AllowedFrom(IncidentStatus from) =>
        Allowed.TryGetValue(from, out var targets) ? targets : [];
}
