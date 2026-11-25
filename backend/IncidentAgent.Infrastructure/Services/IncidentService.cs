using IncidentAgent.Application.Common;
using IncidentAgent.Application.Incidents;
using IncidentAgent.Domain;
using IncidentAgent.Domain.Entities;
using IncidentAgent.Domain.Enums;
using IncidentAgent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace IncidentAgent.Infrastructure.Services;

public class IncidentService(ApplicationDbContext db) : IIncidentService
{
    private const string UniqueViolation = "23505";

    public async Task<Result<IncidentDetailsDto>> CreateAsync(CreateIncidentRequest request, CancellationToken cancellationToken)
    {
        var errors = IncidentValidator.Validate(request);
        if (!errors.HasErrors)
        {
            await RequireExistsAsync(db.IncidentCategories, request.CategoryId!.Value, "categoryId", "Category", errors, cancellationToken);
            await RequireExistsAsync(db.ApplicationServices, request.ServiceId!.Value, "serviceId", "Service", errors, cancellationToken);
            await RequireExistsAsync(db.Users, request.ReporterId!.Value, "reporterId", "Reporter", errors, cancellationToken);
            await ValidateActorAsync(request.ActorUserId, errors, cancellationToken);
        }
        if (errors.HasErrors)
            return Result<IncidentDetailsDto>.Validation(errors.ToDictionary());

        var now = DateTime.UtcNow;
        var incidentNumber = await NextIncidentNumberAsync(now, cancellationToken);

        var incident = new Incident
        {
            Id = Guid.CreateVersion7(),
            IncidentNumber = incidentNumber,
            Title = request.Title!.Trim(),
            Description = request.Description!.Trim(),
            Priority = request.Priority!.Value,
            Status = IncidentStatus.Open,
            CategoryId = request.CategoryId!.Value,
            ServiceId = request.ServiceId!.Value,
            ReporterId = request.ReporterId!.Value,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
        db.Incidents.Add(incident);
        AddHistory(incident, HistoryAction.Created, null, incidentNumber, request.ActorUserId ?? request.ReporterId, now);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: UniqueViolation })
        {
            db.ChangeTracker.Clear();
            return Result<IncidentDetailsDto>.Conflict($"Incident number {incidentNumber} already exists. Please retry.");
        }

        return await GetByIdAsync(incident.Id, cancellationToken);
    }

    public async Task<Result<IncidentDetailsDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var dto = await db.Incidents.AsNoTracking()
            .Where(i => i.Id == id)
            .Select(i => new IncidentDetailsDto(
                i.Id,
                i.IncidentNumber,
                i.Title,
                i.Description,
                i.Priority,
                i.Status,
                new LookupDto(i.Category.Id, i.Category.Name),
                new LookupDto(i.Service.Id, i.Service.Name),
                new UserSummaryDto(i.Reporter.Id, i.Reporter.FullName),
                i.AssignedTo == null ? null : new UserSummaryDto(i.AssignedTo.Id, i.AssignedTo.FullName),
                i.CreatedAtUtc,
                i.UpdatedAtUtc,
                i.ResolvedAtUtc,
                i.ClosedAtUtc,
                i.Comments
                    .OrderBy(c => c.CreatedAtUtc)
                    .Select(c => new CommentDto(c.Id, c.Text, c.AuthorId, c.Author == null ? null : c.Author.FullName, c.CreatedAtUtc))
                    .ToList()))
            .SingleOrDefaultAsync(cancellationToken);

        return dto is null
            ? Result<IncidentDetailsDto>.NotFound($"Incident {id} was not found.")
            : Result<IncidentDetailsDto>.Success(dto);
    }

    public async Task<Result<PagedResult<IncidentListItemDto>>> ListAsync(IncidentListQuery query, CancellationToken cancellationToken)
    {
        var errors = IncidentValidator.Validate(query);
        if (errors.HasErrors)
            return Result<PagedResult<IncidentListItemDto>>.Validation(errors.ToDictionary());

        var page = query.Page ?? 1;
        var pageSize = query.PageSize ?? IncidentValidator.DefaultPageSize;

        var incidents = db.Incidents.AsNoTracking();
        if (query.Status is not null) incidents = incidents.Where(i => i.Status == query.Status);
        if (query.Priority is not null) incidents = incidents.Where(i => i.Priority == query.Priority);
        if (query.CategoryId is not null) incidents = incidents.Where(i => i.CategoryId == query.CategoryId);
        if (query.ServiceId is not null) incidents = incidents.Where(i => i.ServiceId == query.ServiceId);
        if (query.AssignedToId is not null) incidents = incidents.Where(i => i.AssignedToId == query.AssignedToId);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = $"%{EscapeLike(query.Search.Trim())}%";
            incidents = incidents.Where(i =>
                EF.Functions.ILike(i.IncidentNumber, pattern, "\\") ||
                EF.Functions.ILike(i.Title, pattern, "\\"));
        }

        var totalCount = await incidents.CountAsync(cancellationToken);

        var items = await incidents
            .OrderByDescending(i => i.CreatedAtUtc)
            .ThenByDescending(i => i.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new IncidentListItemDto(
                i.Id,
                i.IncidentNumber,
                i.Title,
                i.Priority,
                i.Status,
                i.CategoryId,
                i.Category.Name,
                i.ServiceId,
                i.Service.Name,
                i.ReporterId,
                i.Reporter.FullName,
                i.AssignedToId,
                i.AssignedTo == null ? null : i.AssignedTo.FullName,
                i.CreatedAtUtc,
                i.UpdatedAtUtc))
            .ToListAsync(cancellationToken);

        return Result<PagedResult<IncidentListItemDto>>.Success(new PagedResult<IncidentListItemDto>(items, totalCount, page, pageSize));
    }

    public async Task<Result<IncidentDetailsDto>> UpdateAsync(Guid id, UpdateIncidentRequest request, CancellationToken cancellationToken)
    {
        var incident = await db.Incidents
            .Include(i => i.Category)
            .Include(i => i.Service)
            .SingleOrDefaultAsync(i => i.Id == id, cancellationToken);
        if (incident is null)
            return Result<IncidentDetailsDto>.NotFound($"Incident {id} was not found.");

        var errors = IncidentValidator.Validate(request);
        if (errors.HasErrors)
            return Result<IncidentDetailsDto>.Validation(errors.ToDictionary());

        if (incident.Status == IncidentStatus.Closed)
            return Result<IncidentDetailsDto>.Conflict("Closed incidents cannot be updated.");

        var newCategory = await db.IncidentCategories.SingleOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken);
        var newService = await db.ApplicationServices.SingleOrDefaultAsync(s => s.Id == request.ServiceId, cancellationToken);
        if (newCategory is null) errors.Add("categoryId", "Category does not exist.");
        if (newService is null) errors.Add("serviceId", "Service does not exist.");
        await ValidateActorAsync(request.ActorUserId, errors, cancellationToken);
        if (errors.HasErrors)
            return Result<IncidentDetailsDto>.Validation(errors.ToDictionary());

        var now = DateTime.UtcNow;
        var actor = request.ActorUserId;

        if (incident.Priority != request.Priority)
        {
            AddHistory(incident, HistoryAction.PriorityChanged, incident.Priority.ToString(), request.Priority!.Value.ToString(), actor, now);
            incident.Priority = request.Priority.Value;
        }
        if (incident.CategoryId != newCategory!.Id)
        {
            AddHistory(incident, HistoryAction.CategoryChanged, incident.Category.Name, newCategory.Name, actor, now);
            incident.CategoryId = newCategory.Id;
        }
        if (incident.ServiceId != newService!.Id)
        {
            AddHistory(incident, HistoryAction.ServiceChanged, incident.Service.Name, newService.Name, actor, now);
            incident.ServiceId = newService.Id;
        }

        incident.Title = request.Title!.Trim();
        incident.Description = request.Description!.Trim();
        incident.UpdatedAtUtc = now;

        await db.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<Result<IncidentDetailsDto>> AssignAsync(Guid id, AssignIncidentRequest request, CancellationToken cancellationToken)
    {
        var incident = await db.Incidents
            .Include(i => i.AssignedTo)
            .SingleOrDefaultAsync(i => i.Id == id, cancellationToken);
        if (incident is null)
            return Result<IncidentDetailsDto>.NotFound($"Incident {id} was not found.");

        var errors = IncidentValidator.Validate(request);
        if (errors.HasErrors)
            return Result<IncidentDetailsDto>.Validation(errors.ToDictionary());

        if (incident.Status == IncidentStatus.Closed)
            return Result<IncidentDetailsDto>.Conflict("Closed incidents cannot be assigned.");

        var assignee = await db.Users.SingleOrDefaultAsync(u => u.Id == request.AssigneeId, cancellationToken);
        if (assignee is null) errors.Add("assigneeId", "Assignee does not exist.");
        await ValidateActorAsync(request.ActorUserId, errors, cancellationToken);
        if (errors.HasErrors)
            return Result<IncidentDetailsDto>.Validation(errors.ToDictionary());

        if (incident.AssignedToId == assignee!.Id)
            return Result<IncidentDetailsDto>.Conflict($"Incident is already assigned to {assignee.FullName}.");

        var now = DateTime.UtcNow;
        var actor = request.ActorUserId;

        AddHistory(incident, HistoryAction.Assigned, incident.AssignedTo?.FullName, assignee.FullName, actor, now);
        db.IncidentAssignments.Add(new IncidentAssignment
        {
            Id = Guid.CreateVersion7(),
            IncidentId = incident.Id,
            AssigneeId = assignee.Id,
            AssignedById = actor,
            AssignedAtUtc = now
        });
        incident.AssignedToId = assignee.Id;

        // Assigning an Open incident moves it to Assigned.
        if (incident.Status == IncidentStatus.Open)
        {
            AddHistory(incident, HistoryAction.StatusChanged, nameof(IncidentStatus.Open), nameof(IncidentStatus.Assigned), actor, now);
            incident.Status = IncidentStatus.Assigned;
        }

        incident.UpdatedAtUtc = now;
        await db.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<Result<IncidentDetailsDto>> ChangeStatusAsync(Guid id, ChangeStatusRequest request, CancellationToken cancellationToken)
    {
        var incident = await db.Incidents.SingleOrDefaultAsync(i => i.Id == id, cancellationToken);
        if (incident is null)
            return Result<IncidentDetailsDto>.NotFound($"Incident {id} was not found.");

        var errors = IncidentValidator.Validate(request);
        await ValidateActorAsync(request.ActorUserId, errors, cancellationToken);
        if (errors.HasErrors)
            return Result<IncidentDetailsDto>.Validation(errors.ToDictionary());

        var from = incident.Status;
        var to = request.NewStatus!.Value;

        if (!IncidentStatusTransitions.CanTransition(from, to))
        {
            var allowed = IncidentStatusTransitions.AllowedFrom(from);
            var allowedText = allowed.Count == 0 ? "none" : string.Join(", ", allowed);
            return Result<IncidentDetailsDto>.Conflict($"Cannot change status from {from} to {to}. Allowed: {allowedText}.");
        }

        if (to == IncidentStatus.Assigned && incident.AssignedToId is null)
            return Result<IncidentDetailsDto>.Conflict("Assign the incident to a user before setting status to Assigned.");

        var now = DateTime.UtcNow;
        AddHistory(incident, HistoryAction.StatusChanged, from.ToString(), to.ToString(), request.ActorUserId, now, request.Note?.Trim());

        incident.Status = to;
        if (to == IncidentStatus.Resolved) incident.ResolvedAtUtc = now;
        if (to == IncidentStatus.Closed) incident.ClosedAtUtc = now;
        if (from == IncidentStatus.Resolved && to == IncidentStatus.Investigating) incident.ResolvedAtUtc = null; // reopened
        incident.UpdatedAtUtc = now;

        await db.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<Result<CommentDto>> AddCommentAsync(Guid id, AddCommentRequest request, CancellationToken cancellationToken)
    {
        var incident = await db.Incidents.SingleOrDefaultAsync(i => i.Id == id, cancellationToken);
        if (incident is null)
            return Result<CommentDto>.NotFound($"Incident {id} was not found.");

        var errors = IncidentValidator.Validate(request);
        await ValidateActorAsync(request.ActorUserId, errors, cancellationToken);
        if (errors.HasErrors)
            return Result<CommentDto>.Validation(errors.ToDictionary());

        if (incident.Status == IncidentStatus.Closed)
            return Result<CommentDto>.Conflict("Closed incidents cannot be commented on.");

        var now = DateTime.UtcNow;
        var comment = new IncidentComment
        {
            Id = Guid.CreateVersion7(),
            IncidentId = incident.Id,
            AuthorId = request.ActorUserId,
            Text = request.Text!.Trim(),
            CreatedAtUtc = now
        };
        db.IncidentComments.Add(comment);
        incident.UpdatedAtUtc = now;
        await db.SaveChangesAsync(cancellationToken);

        var authorName = request.ActorUserId is null
            ? null
            : await db.Users.Where(u => u.Id == request.ActorUserId).Select(u => u.FullName).SingleAsync(cancellationToken);

        return Result<CommentDto>.Success(new CommentDto(comment.Id, comment.Text, comment.AuthorId, authorName, comment.CreatedAtUtc));
    }

    public async Task<Result<IReadOnlyList<IncidentHistoryDto>>> GetHistoryAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!await db.Incidents.AnyAsync(i => i.Id == id, cancellationToken))
            return Result<IReadOnlyList<IncidentHistoryDto>>.NotFound($"Incident {id} was not found.");

        var history = await db.IncidentHistory.AsNoTracking()
            .Where(h => h.IncidentId == id)
            .OrderBy(h => h.ChangedAtUtc)
            .ThenBy(h => h.Id)
            .Select(h => new IncidentHistoryDto(
                h.Id, h.Action, h.OldValue, h.NewValue, h.Note, h.ActorUserId,
                h.ActorUser == null ? null : h.ActorUser.FullName, h.ChangedAtUtc))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<IncidentHistoryDto>>.Success(history);
    }

    private async Task<string> NextIncidentNumberAsync(DateTime nowUtc, CancellationToken cancellationToken)
    {
        var value = await db.Database
            .SqlQueryRaw<long>($"SELECT nextval('{ApplicationDbContext.IncidentNumberSequence}') AS \"Value\"")
            .SingleAsync(cancellationToken);
        return $"INC-{nowUtc.Year}-{value:D6}";
    }

    private DateTime _lastHistoryTimeUtc = DateTime.MinValue;

    private void AddHistory(Incident incident, HistoryAction action, string? oldValue, string? newValue,
        Guid? actorUserId, DateTime nowUtc, string? note = null)
    {
        // Rows written in the same operation get strictly increasing timestamps (1 µs apart),
        // so "oldest first" keeps the order in which the changes were made.
        if (nowUtc <= _lastHistoryTimeUtc)
            nowUtc = _lastHistoryTimeUtc.AddTicks(10);
        _lastHistoryTimeUtc = nowUtc;

        db.IncidentHistory.Add(new IncidentHistory
        {
            Id = Guid.CreateVersion7(),
            IncidentId = incident.Id,
            Action = action,
            OldValue = oldValue,
            NewValue = newValue,
            Note = string.IsNullOrWhiteSpace(note) ? null : note,
            ActorUserId = actorUserId,
            ChangedAtUtc = nowUtc
        });
    }

    private async Task ValidateActorAsync(Guid? actorUserId, ValidationErrors errors, CancellationToken cancellationToken)
    {
        if (actorUserId is not null && !await db.Users.AnyAsync(u => u.Id == actorUserId, cancellationToken))
            errors.Add("actorUserId", "Actor user does not exist.");
    }

    private static async Task RequireExistsAsync<T>(DbSet<T> set, Guid id, string field, string label,
        ValidationErrors errors, CancellationToken cancellationToken) where T : class
    {
        if (await set.FindAsync([id], cancellationToken) is null)
            errors.Add(field, $"{label} does not exist.");
    }

    private static string EscapeLike(string value) =>
        value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}
