using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Data;
using TaskFlow.Api.DTOs;
using TaskFlow.Api.Models;
using DomainTaskStatus = TaskFlow.Api.Models.TaskStatus;
using DomainTaskPriority = TaskFlow.Api.Models.TaskPriority;


namespace TaskFlow.Api.Services;


public class TaskService
{
    private readonly AppDbContext _db;
    public TaskService(AppDbContext db) => _db = db;
    private static readonly IReadOnlyDictionary<DomainTaskStatus, DomainTaskStatus[]> AllowedTransitions =
        new Dictionary<DomainTaskStatus, DomainTaskStatus[]>
        {
            [DomainTaskStatus.Todo] = new[]
            {
                DomainTaskStatus.InProgress,
                DomainTaskStatus.Cancelled
            },

            [DomainTaskStatus.InProgress] = new[]
            {
                DomainTaskStatus.Done,
                DomainTaskStatus.Cancelled
            },

            [DomainTaskStatus.Done] = Array.Empty<DomainTaskStatus>(),

            [DomainTaskStatus.Cancelled] = Array.Empty<DomainTaskStatus>()
        };



    public async Task<PagedResponse<TaskResponse>> SearchAsync(
        Guid? projectId,
        TaskStatusDto? status,
        TaskPriorityDto? priority,
        string? tag,
        string? search,
        int page,
        int pageSize)
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize is <= 0 or > 100 ? 20 : pageSize;

        var q = _db.Tasks
            .AsNoTracking()
            .Include(t => t.TaskTags)
                .ThenInclude(tt => tt.Tag)
            .AsQueryable();

        if (projectId.HasValue)
            q = q.Where(x => x.ProjectId == projectId.Value);

        if (status.HasValue)
        {
            var st = MapStatus(status.Value);
            q = q.Where(x => x.Status == st);
        }

        if (priority.HasValue)
        {
            var pr = MapPriority(priority.Value);
            q = q.Where(x => x.Priority == pr);
        }

        if (!string.IsNullOrWhiteSpace(tag))
        {
            var tg = tag.Trim().ToLowerInvariant();
            q = q.Where(x => x.TaskTags.Any(tt => tt.Tag.Name.ToLower() == tg));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLowerInvariant();
            q = q.Where(x =>
                x.Title.ToLower().Contains(s) ||
                (x.Description != null && x.Description.ToLower().Contains(s)));
        }

        var total = await q.CountAsync();

        var items = await q
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new TaskResponse(
                x.Id,
                x.ProjectId,
                x.Title,
                x.Description,
                MapStatus(x.Status),
                MapPriority(x.Priority),
                x.DueDate,
                x.CreatedAtUtc,
                x.UpdatedAtUtc,
                x.TaskTags.Select(tt => tt.Tag.Name).OrderBy(n => n).ToList()
            ))
            .ToListAsync();

        return new PagedResponse<TaskResponse>(items, total, page, pageSize);
    }

    public async Task<TaskResponse> CreateAsync(CreateTaskRequest req)
    {
        var projectExists = await _db.Projects.AnyAsync(p => p.Id == req.ProjectId);
        if (!projectExists) throw new KeyNotFoundException("Project not found.");

        var task = new TaskItem
        {
            ProjectId = req.ProjectId,
            Title = req.Title.Trim(),
            Description = req.Description?.Trim(),
            Priority = MapPriority(req.Priority),
            DueDate = req.DueDate,
            Status = DomainTaskStatus.Todo
            
        };

        if (req.Tags is { Count: > 0 })
        {
            var tags = await UpsertTagsAsync(req.Tags);
            foreach (var t in tags)
                task.TaskTags.Add(new TaskTag { TagId = t.Id, TaskItemId = task.Id });
        }

        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();

        return await GetByIdAsync(task.Id);
    }

    public async Task<TaskResponse> GetByIdAsync(Guid id)
    {
        var task = await _db.Tasks
            .AsNoTracking()
            .Include(t => t.TaskTags).ThenInclude(tt => tt.Tag)
            .FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new KeyNotFoundException("Task not found.");

        return new TaskResponse(
            task.Id,
            task.ProjectId,
            task.Title,
            task.Description,
            MapStatus(task.Status),
            MapPriority(task.Priority),
            task.DueDate,
            task.CreatedAtUtc,
            task.UpdatedAtUtc,
            task.TaskTags.Select(tt => tt.Tag.Name).OrderBy(n => n).ToList()
        );
    }

    public async Task<TaskResponse> UpdateAsync(Guid id, UpdateTaskRequest req)
    {
        var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new KeyNotFoundException("Task not found.");

        task.Title = req.Title.Trim();
        task.Description = req.Description?.Trim();
        task.Priority = MapPriority(req.Priority);
        task.DueDate = req.DueDate;
        task.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<TaskResponse> UpdateStatusAsync(Guid id, UpdateTaskStatusRequest req)
    {
        var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == id)
                   ?? throw new KeyNotFoundException("Task not found.");

        var newStatus = MapStatus(req.Status);
        var currentStatus = task.Status;

        if (currentStatus == newStatus)
            return await GetByIdAsync(id);

        if (!AllowedTransitions.TryGetValue(currentStatus, out var allowedNext) ||
            !allowedNext.Contains(newStatus))
        {
            throw new InvalidOperationException(
                $"Status transition '{currentStatus} → {newStatus}' is not allowed.");
        }

        task.Status = newStatus;
        task.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return await GetByIdAsync(id);
    }


    public async Task<CommentResponse> AddCommentAsync(Guid taskId, AddCommentRequest req)
    {
        var exists = await _db.Tasks.AnyAsync(t => t.Id == taskId);
        if (!exists) throw new KeyNotFoundException("Task not found.");

        var c = new Comment { TaskItemId = taskId, Text = req.Text.Trim() };
        _db.Comments.Add(c);
        await _db.SaveChangesAsync();

        return new CommentResponse(c.Id, c.TaskItemId, c.Text, c.CreatedAtUtc);
    }

    public async Task<TaskResponse> SetTagsAsync(Guid taskId, List<string> tags)
    {
        var task = await _db.Tasks
            .Include(t => t.TaskTags)
            .FirstOrDefaultAsync(t => t.Id == taskId)
            ?? throw new KeyNotFoundException("Task not found.");

        task.TaskTags.Clear();

        if (tags.Count > 0)
        {
            var upserted = await UpsertTagsAsync(tags);
            foreach (var tg in upserted)
                task.TaskTags.Add(new TaskTag { TaskItemId = task.Id, TagId = tg.Id });
        }

        task.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return await GetByIdAsync(taskId);
    }

    private async Task<List<Tag>> UpsertTagsAsync(List<string> rawTags)
    {
        var normalized = rawTags
            .Select(t => t.Trim())
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.ToLowerInvariant())
            .Distinct()
            .Take(20)
            .ToList();

        if (normalized.Count == 0) return new();

        var existing = await _db.Tags.Where(t => normalized.Contains(t.Name)).ToListAsync();

        var missing = normalized.Except(existing.Select(e => e.Name)).ToList();
        foreach (var name in missing)
            existing.Add(new Tag { Name = name });

        if (missing.Count > 0)
            await _db.SaveChangesAsync();

        return existing;
    }

    private static DomainTaskStatus MapStatus(TaskStatusDto dto) => dto switch
    {
        TaskStatusDto.Todo => DomainTaskStatus.Todo,
        TaskStatusDto.InProgress => DomainTaskStatus.InProgress,
        TaskStatusDto.Done => DomainTaskStatus.Done,
        TaskStatusDto.Cancelled => DomainTaskStatus.Cancelled,
        _ => throw new ArgumentException("Unknown status")
    };

    private static TaskStatusDto MapStatus(DomainTaskStatus domain) => domain switch
    {
        DomainTaskStatus.Todo => TaskStatusDto.Todo,
        DomainTaskStatus.InProgress => TaskStatusDto.InProgress,
        DomainTaskStatus.Done => TaskStatusDto.Done,
        DomainTaskStatus.Cancelled => TaskStatusDto.Cancelled,
        _ => throw new ArgumentException("Unknown status")
    };

    private static DomainTaskPriority MapPriority(TaskPriorityDto dto) => dto switch
    {
        TaskPriorityDto.Low => DomainTaskPriority.Low,
        TaskPriorityDto.Medium => DomainTaskPriority.Medium,
        TaskPriorityDto.High => DomainTaskPriority.High,
        TaskPriorityDto.Critical => DomainTaskPriority.Critical,
        _ => throw new ArgumentException("Unknown priority")
    };

    private static TaskPriorityDto MapPriority(DomainTaskPriority domain) => domain switch
    {
        DomainTaskPriority.Low => TaskPriorityDto.Low,
        DomainTaskPriority.Medium => TaskPriorityDto.Medium,
        DomainTaskPriority.High => TaskPriorityDto.High,
        DomainTaskPriority.Critical => TaskPriorityDto.Critical,
        _ => throw new ArgumentException("Unknown priority")
    };

}
