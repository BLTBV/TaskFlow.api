namespace TaskFlow.Api.Models;

public class TaskItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public string Title { get; set; } = null!;
    public string? Description { get; set; }

    public TaskStatus Status { get; set; } = TaskStatus.Todo;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public DateOnly? DueDate { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public List<Comment> Comments { get; set; } = new();
    public List<TaskTag> TaskTags { get; set; } = new();
}