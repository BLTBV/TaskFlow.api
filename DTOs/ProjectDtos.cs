namespace TaskFlow.Api.DTOs;

public record CreateProjectRequest(string Name, string? Description);
public record UpdateProjectRequest(string Name, string? Description);

public record ProjectResponse(
    Guid Id,
    string Name,
    string? Description,
    DateTime CreatedAtUtc
);