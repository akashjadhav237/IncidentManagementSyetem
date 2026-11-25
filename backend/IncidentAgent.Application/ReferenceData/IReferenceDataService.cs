namespace IncidentAgent.Application.ReferenceData;

public interface IReferenceDataService
{
    Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<ServiceDto>> GetServicesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<UserDto>> GetUsersAsync(CancellationToken cancellationToken);
}
