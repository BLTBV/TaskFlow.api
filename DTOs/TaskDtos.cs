namespace TaskFlow.Api.DTOs;

public enum TaskStatusDto
{
    Todo = 0,
    InProgress = 1,
    Done = 2,
    Cancelled = 3
}

public enum TaskPriorityDto
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

public record CreateTaskRequest(
    Guid ProjectId,
    string Title,
    string? Description,
    TaskPriorityDto Priority,
    DateOnly? DueDate,
    List<string>? Tags
);

public record UpdateTaskRequest(
    string Title,
    string? Description,
    TaskPriorityDto Priority,
    DateOnly? DueDate
);

public record UpdateTaskStatusRequest(TaskStatusDto Status);

public record TaskResponse(
    Guid Id,
    Guid ProjectId,
    string Title,
    string? Description,
    TaskStatusDto Status,
    TaskPriorityDto Priority,
    DateOnly? DueDate,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    List<string> Tags
);

public record PagedResponse<T>(List<T> Items, int TotalCount, int Page, int PageSize);