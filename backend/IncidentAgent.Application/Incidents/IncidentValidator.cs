using IncidentAgent.Application.Common;
using IncidentAgent.Domain.Enums;

namespace IncidentAgent.Application.Incidents;

/// <summary>Shape validation of requests (no database access). Existence checks happen in the service.</summary>
public static class IncidentValidator
{
    public const int TitleMaxLength = 200;
    public const int DescriptionMaxLength = 4000;
    public const int CommentMaxLength = 4000;
    public const int NoteMaxLength = 1000;
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public static ValidationErrors Validate(CreateIncidentRequest r)
    {
        var errors = new ValidationErrors();
        ValidateCommon(errors, r.Title, r.Description, r.Priority, r.CategoryId, r.ServiceId);
        RequireId(errors, "reporterId", r.ReporterId);
        return errors;
    }

    public static ValidationErrors Validate(UpdateIncidentRequest r)
    {
        var errors = new ValidationErrors();
        ValidateCommon(errors, r.Title, r.Description, r.Priority, r.CategoryId, r.ServiceId);
        return errors;
    }

    public static ValidationErrors Validate(AssignIncidentRequest r)
    {
        var errors = new ValidationErrors();
        RequireId(errors, "assigneeId", r.AssigneeId);
        return errors;
    }

    public static ValidationErrors Validate(ChangeStatusRequest r)
    {
        var errors = new ValidationErrors();
        if (r.NewStatus is null || !Enum.IsDefined(r.NewStatus.Value))
            errors.Add("newStatus", "A valid status is required.");
        if (r.Note is { Length: > NoteMaxLength })
            errors.Add("note", $"Note must be at most {NoteMaxLength} characters.");
        return errors;
    }

    public static ValidationErrors Validate(AddCommentRequest r)
    {
        var errors = new ValidationErrors();
        if (string.IsNullOrWhiteSpace(r.Text))
            errors.Add("text", "Comment text is required.");
        else if (r.Text.Length > CommentMaxLength)
            errors.Add("text", $"Comment text must be at most {CommentMaxLength} characters.");
        return errors;
    }

    public static ValidationErrors Validate(IncidentListQuery q)
    {
        var errors = new ValidationErrors();
        if (q.Page is < 1)
            errors.Add("page", "Page must be 1 or greater.");
        if (q.PageSize is < 1 or > MaxPageSize)
            errors.Add("pageSize", $"Page size must be between 1 and {MaxPageSize}.");
        return errors;
    }

    private static void ValidateCommon(ValidationErrors errors, string? title, string? description,
        Priority? priority, Guid? categoryId, Guid? serviceId)
    {
        if (string.IsNullOrWhiteSpace(title))
            errors.Add("title", "Title is required.");
        else if (title.Trim().Length > TitleMaxLength)
            errors.Add("title", $"Title must be at most {TitleMaxLength} characters.");

        if (string.IsNullOrWhiteSpace(description))
            errors.Add("description", "Description is required.");
        else if (description.Trim().Length > DescriptionMaxLength)
            errors.Add("description", $"Description must be at most {DescriptionMaxLength} characters.");

        if (priority is null || !Enum.IsDefined(priority.Value))
            errors.Add("priority", "A valid priority is required (P1Critical, P2High, P3Medium, P4Low).");

        RequireId(errors, "categoryId", categoryId);
        RequireId(errors, "serviceId", serviceId);
    }

    private static void RequireId(ValidationErrors errors, string field, Guid? id)
    {
        if (id is null || id == Guid.Empty)
            errors.Add(field, $"{field} is required.");
    }
}
