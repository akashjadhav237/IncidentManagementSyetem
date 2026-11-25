namespace IncidentAgent.BlazorUi.Models;

/// <summary>Filters and paging for GET /api/incidents.</summary>
public sealed record IncidentQuery
{
    public const int DefaultPageSize = 20;
    public static readonly int[] PageSizes = [10, 20, 50];

    public string? Search { get; init; }
    public string? Status { get; init; }
    public string? Priority { get; init; }
    public Guid? CategoryId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = DefaultPageSize;

    public string ToQueryString()
    {
        var parts = new List<string> { $"page={Page}", $"pageSize={PageSize}" };
        if (!string.IsNullOrWhiteSpace(Search)) parts.Add($"search={Uri.EscapeDataString(Search.Trim())}");
        if (!string.IsNullOrWhiteSpace(Status)) parts.Add($"status={Uri.EscapeDataString(Status)}");
        if (!string.IsNullOrWhiteSpace(Priority)) parts.Add($"priority={Uri.EscapeDataString(Priority)}");
        if (CategoryId is not null) parts.Add($"categoryId={CategoryId}");
        return "?" + string.Join("&", parts);
    }
}

/// <summary>Values emitted by the IncidentFilters component.</summary>
public record IncidentFilterValues(string? Search, string? Status, string? Priority, Guid? CategoryId);

public record DashboardCounts(int Total, int Open, int InProgress, int Critical, int Resolved, int P1, int P2, int P3, int P4);

public record Crumb(string Label, string? Href = null);

public enum StateKind
{
    Loading,
    Empty,
    Error
}
