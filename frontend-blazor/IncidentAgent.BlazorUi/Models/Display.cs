using System.Globalization;

namespace IncidentAgent.BlazorUi.Models;

/// <summary>Labels, CSS classes and date formatting shared by components.</summary>
public static class Display
{
    public static readonly string[] Statuses = ["Open", "Assigned", "Investigating", "Pending", "Resolved", "Closed"];
    public static readonly string[] Priorities = ["P1Critical", "P2High", "P3Medium", "P4Low"];

    public static string PriorityLabel(string? priority) => priority switch
    {
        "P1Critical" => "P1 Critical",
        "P2High" => "P2 High",
        "P3Medium" => "P3 Medium",
        "P4Low" => "P4 Low",
        _ => priority ?? "—"
    };

    public static string PriorityCss(string? priority) => priority switch
    {
        "P1Critical" => "p1",
        "P2High" => "p2",
        "P3Medium" => "p3",
        _ => "p4"
    };

    public static string StatusCss(string? status) => "status-" + (status ?? "unknown").ToLowerInvariant();

    /// <summary>UTC from the API -> browser local time, e.g. "24 Nov 2026, 14:05".</summary>
    public static string LocalTime(DateTime utc) =>
        DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToLocalTime().ToString("d MMM yyyy, HH:mm", CultureInfo.InvariantCulture);

    public static string LocalTime(DateTime? utc) => utc is null ? "—" : LocalTime(utc.Value);

    public static string HistoryTitle(IncidentHistoryEntry entry) => entry.Action switch
    {
        "Created" => "Incident created",
        "StatusChanged" => "Status changed",
        "PriorityChanged" => "Priority changed",
        "Assigned" => "Assigned",
        "CategoryChanged" => "Category changed",
        "ServiceChanged" => "Service changed",
        _ => entry.Action
    };

    public static string HistoryValue(IncidentHistoryEntry entry, string? value) =>
        entry.Action == "PriorityChanged" ? PriorityLabel(value) : value ?? "—";
}
