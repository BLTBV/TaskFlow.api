namespace TaskFlow.Api.Models;

public class Comment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TaskItemId { get; set; }
    public TaskItem TaskItem { get; set; } = null!;

    public string Text { get; set; } = null!;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}