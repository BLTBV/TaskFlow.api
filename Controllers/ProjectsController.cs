using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.DTOs;
using TaskFlow.Api.Services;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Route("api/projects")]
public class ProjectsController : ControllerBase
{
    private readonly ProjectService _service;
    public ProjectsController(ProjectService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<List<ProjectResponse>>> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpPost]
    public async Task<ActionResult<ProjectResponse>> Create(CreateProjectRequest req)
        => Ok(await _service.CreateAsync(req));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProjectResponse>> Update(Guid id, UpdateProjectRequest req)
        => Ok(await _service.UpdateAsync(id, req));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}