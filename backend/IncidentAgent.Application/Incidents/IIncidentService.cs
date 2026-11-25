using IncidentAgent.Application.Common;

namespace IncidentAgent.Application.Incidents;

public interface IIncidentService
{
    Task<Result<IncidentDetailsDto>> CreateAsync(CreateIncidentRequest request, CancellationToken cancellationToken);
    Task<Result<IncidentDetailsDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Result<PagedResult<IncidentListItemDto>>> ListAsync(IncidentListQuery query, CancellationToken cancellationToken);
    Task<Result<IncidentDetailsDto>> UpdateAsync(Guid id, UpdateIncidentRequest request, CancellationToken cancellationToken);
    Task<Result<IncidentDetailsDto>> AssignAsync(Guid id, AssignIncidentRequest request, CancellationToken cancellationToken);
    Task<Result<IncidentDetailsDto>> ChangeStatusAsync(Guid id, ChangeStatusRequest request, CancellationToken cancellationToken);
    Task<Result<CommentDto>> AddCommentAsync(Guid id, AddCommentRequest request, CancellationToken cancellationToken);
    Task<Result<IReadOnlyList<IncidentHistoryDto>>> GetHistoryAsync(Guid id, CancellationToken cancellationToken);
}
