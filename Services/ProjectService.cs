using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Data;
using TaskFlow.Api.DTOs;
using TaskFlow.Api.Models;

namespace TaskFlow.Api.Services;

public class ProjectService
{
    private readonly AppDbContext _db;
    public ProjectService(AppDbContext db) => _db = db;

    public async Task<List<ProjectResponse>> GetAllAsync()
    {
        return await _db.Projects
            .OrderByDescending(p => p.CreatedAtUtc)
            .Select(p => new ProjectResponse(p.Id, p.Name, p.Description, p.CreatedAtUtc))
            .ToListAsync();
    }

    public async Task<ProjectResponse> CreateAsync(CreateProjectRequest req)
    {
        var project = new Project { Name = req.Name.Trim(), Description = req.Description?.Trim() };
        _db.Projects.Add(project);
        await _db.SaveChangesAsync();
        return new ProjectResponse(project.Id, project.Name, project.Description, project.CreatedAtUtc);
    }

    public async Task<ProjectResponse> UpdateAsync(Guid id, UpdateProjectRequest req)
    {
        var project = await _db.Projects.FirstOrDefaultAsync(x => x.Id == id)
                      ?? throw new KeyNotFoundException("Project not found.");

        project.Name = req.Name.Trim();
        project.Description = req.Description?.Trim();

        await _db.SaveChangesAsync();
        return new ProjectResponse(project.Id, project.Name, project.Description, project.CreatedAtUtc);
    }

    public async Task DeleteAsync(Guid id)
    {
        var project = await _db.Projects.FirstOrDefaultAsync(x => x.Id == id)
                      ?? throw new KeyNotFoundException("Project not found.");

        _db.Projects.Remove(project);
        await _db.SaveChangesAsync();
    }
}