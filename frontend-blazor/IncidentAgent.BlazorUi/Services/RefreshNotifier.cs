namespace IncidentAgent.BlazorUi.Services;

/// <summary>Lets the top bar "Refresh" button ask the current page to reload its data.</summary>
public sealed class RefreshNotifier
{
    public event Action? RefreshRequested;

    public void RequestRefresh() => RefreshRequested?.Invoke();
}
