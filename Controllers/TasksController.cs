using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.DTOs;
using TaskFlow.Api.Services;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Route("api/tasks")]
public class TasksController : ControllerBase
{
    private readonly TaskService _service;
    public TasksController(TaskService service) => _service = service;

    // GET /api/tasks?projectId=&status=&priority=&tag=&search=&page=&pageSize=
    [HttpGet]
    public async Task<ActionResult<PagedResponse<TaskResponse>>> Search(
        [FromQuery] Guid? projectId,
        [FromQuery] TaskStatusDto? status,
        [FromQuery] TaskPriorityDto? priority,
        [FromQuery] string? tag,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
        => Ok(await _service.SearchAsync(projectId, status, priority, tag, search, page, pageSize));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TaskResponse>> GetById(Guid id)
        => Ok(await _service.GetByIdAsync(id));

    [HttpPost]
    public async Task<ActionResult<TaskResponse>> Create(CreateTaskRequest req)
        => Ok(await _service.CreateAsync(req));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TaskResponse>> Update(Guid id, UpdateTaskRequest req)
        => Ok(await _service.UpdateAsync(id, req));

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<TaskResponse>> UpdateStatus(Guid id, UpdateTaskStatusRequest req)
        => Ok(await _service.UpdateStatusAsync(id, req));

    [HttpPost("{id:guid}/comments")]
    public async Task<ActionResult<CommentResponse>> AddComment(Guid id, AddCommentRequest req)
        => Ok(await _service.AddCommentAsync(id, req));

    // Replace all tags for task
    [HttpPut("{id:guid}/tags")]
    public async Task<ActionResult<TaskResponse>> SetTags(Guid id, List<string> tags)
        => Ok(await _service.SetTagsAsync(id, tags));
}