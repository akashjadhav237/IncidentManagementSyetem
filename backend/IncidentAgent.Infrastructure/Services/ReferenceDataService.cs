using IncidentAgent.Application.ReferenceData;
using IncidentAgent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IncidentAgent.Infrastructure.Services;

public class ReferenceDataService(ApplicationDbContext db) : IReferenceDataService
{
    public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken) =>
        await db.IncidentCategories.AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Description))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ServiceDto>> GetServicesAsync(CancellationToken cancellationToken) =>
        await db.ApplicationServices.AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => new ServiceDto(s.Id, s.Name, s.Description, s.OwnerTeam))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<UserDto>> GetUsersAsync(CancellationToken cancellationToken) =>
        await db.Users.AsNoTracking()
            .OrderBy(u => u.FullName)
            .Select(u => new UserDto(u.Id, u.FullName, u.Email))
            .ToListAsync(cancellationToken);
}
