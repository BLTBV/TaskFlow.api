namespace TaskFlow.Api.Models;

public enum TaskStatus
{
    Todo = 0,
    InProgress = 1,
    Done = 2,
    Cancelled = 3
}

public enum TaskPriority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}