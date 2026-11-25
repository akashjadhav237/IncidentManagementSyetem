using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using IncidentAgent.BlazorUi.Models;

namespace IncidentAgent.BlazorUi.Services;

/// <summary>
/// Read-only client for the IncidentAgent API. Uses the HttpClient registered in Program.cs (BaseAddress = ApiBaseUrl).
/// Pages get their own instance via OwningComponentBase, so disposing the page cancels its in-flight requests.
/// </summary>
public sealed class IncidentApiService(HttpClient http) : IDisposable
{
    private readonly CancellationTokenSource _disposeCts = new();
    private bool _disposed;

    public Task<ApiResult<PagedResult<IncidentListItem>>> GetIncidentsAsync(IncidentQuery query, CancellationToken cancellationToken = default) =>
        GetAsync<PagedResult<IncidentListItem>>($"api/incidents{query.ToQueryString()}", cancellationToken);

    public Task<ApiResult<IncidentDetail>> GetIncidentAsync(Guid id, CancellationToken cancellationToken = default) =>
        GetAsync<IncidentDetail>($"api/incidents/{id}", cancellationToken, notFoundMessage: "This incident does not exist or was removed.");

    public Task<ApiResult<IReadOnlyList<IncidentHistoryEntry>>> GetHistoryAsync(Guid id, CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<IncidentHistoryEntry>>($"api/incidents/{id}/history", cancellationToken);

    public Task<ApiResult<IReadOnlyList<Category>>> GetCategoriesAsync(CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<Category>>("api/categories", cancellationToken);

    public Task<ApiResult<IReadOnlyList<ApplicationServiceItem>>> GetServicesAsync(CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<ApplicationServiceItem>>("api/services", cancellationToken);

    public Task<ApiResult<IReadOnlyList<UserItem>>> GetUsersAsync(CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<UserItem>>("api/users", cancellationToken);

    /// <summary>Number of incidents matching a filter, read from totalCount of a pageSize=1 request.</summary>
    public async Task<ApiResult<int>> GetCountAsync(string? status = null, string? priority = null, CancellationToken cancellationToken = default)
    {
        var result = await GetIncidentsAsync(new IncidentQuery { Status = status, Priority = priority, PageSize = 1 }, cancellationToken);
        return result.IsSuccess ? ApiResult<int>.Ok(result.Data!.TotalCount) : new ApiResult<int>(0, result.Error, result.IsCancelled);
    }

    /// <summary>Dashboard figures. All count requests run in parallel.</summary>
    public async Task<ApiResult<DashboardCounts>> GetDashboardCountsAsync(CancellationToken cancellationToken = default)
    {
        var total = GetCountAsync(cancellationToken: cancellationToken);
        var open = GetCountAsync("Open", cancellationToken: cancellationToken);
        var assigned = GetCountAsync("Assigned", cancellationToken: cancellationToken);
        var investigating = GetCountAsync("Investigating", cancellationToken: cancellationToken);
        var resolved = GetCountAsync("Resolved", cancellationToken: cancellationToken);
        var p1 = GetCountAsync(priority: "P1Critical", cancellationToken: cancellationToken);
        var p2 = GetCountAsync(priority: "P2High", cancellationToken: cancellationToken);
        var p3 = GetCountAsync(priority: "P3Medium", cancellationToken: cancellationToken);
        var p4 = GetCountAsync(priority: "P4Low", cancellationToken: cancellationToken);

        var all = await Task.WhenAll(total, open, assigned, investigating, resolved, p1, p2, p3, p4);

        var failed = all.FirstOrDefault(r => !r.IsSuccess);
        if (failed is not null)
            return new ApiResult<DashboardCounts>(null, failed.Error, failed.IsCancelled);

        return ApiResult<DashboardCounts>.Ok(new DashboardCounts(
            Total: total.Result.Data,
            Open: open.Result.Data,
            InProgress: assigned.Result.Data + investigating.Result.Data,
            Critical: p1.Result.Data,
            Resolved: resolved.Result.Data,
            P1: p1.Result.Data,
            P2: p2.Result.Data,
            P3: p3.Result.Data,
            P4: p4.Result.Data));
    }

    private async Task<ApiResult<T>> GetAsync<T>(string url, CancellationToken cancellationToken, string? notFoundMessage = null)
    {
        if (_disposed)
            return ApiResult<T>.Cancelled();

        using var linked = CancellationTokenSource.CreateLinkedTokenSource(_disposeCts.Token, cancellationToken);
        try
        {
            using var response = await http.GetAsync(url, linked.Token);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return ApiResult<T>.Fail(notFoundMessage ?? "The requested data was not found.", notFound: true);

            if (!response.IsSuccessStatusCode)
                return ApiResult<T>.Fail($"The API returned an error ({(int)response.StatusCode} {response.ReasonPhrase}). Please try again.");

            var data = await response.Content.ReadFromJsonAsync<T>(linked.Token);
            return data is null
                ? ApiResult<T>.Fail("The API returned an empty response.")
                : ApiResult<T>.Ok(data);
        }
        catch (OperationCanceledException) when (linked.IsCancellationRequested)
        {
            return ApiResult<T>.Cancelled();
        }
        catch (HttpRequestException)
        {
            return ApiResult<T>.Fail($"Cannot reach the IncidentAgent API at {http.BaseAddress}. Check that the API is running and try again.");
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            return ApiResult<T>.Fail("The API returned data in an unexpected format.");
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _disposeCts.Cancel();
        _disposeCts.Dispose();
    }
}
