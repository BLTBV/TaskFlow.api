namespace TaskFlow.Api.Models;

public class Tag
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = null!;

    public List<TaskTag> TaskTags { get; set; } = new();
}