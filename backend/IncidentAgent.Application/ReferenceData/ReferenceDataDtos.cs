namespace IncidentAgent.Application.ReferenceData;

public record CategoryDto(Guid Id, string Name, string? Description);

public record ServiceDto(Guid Id, string Name, string? Description, string? OwnerTeam);

public record UserDto(Guid Id, string FullName, string Email);
