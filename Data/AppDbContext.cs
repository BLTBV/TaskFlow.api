using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Models;

namespace TaskFlow.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<TaskTag> TaskTags => Set<TaskTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>()
            .HasIndex(x => x.Name)
            .IsUnique(false);

        modelBuilder.Entity<Tag>()
            .HasIndex(x => x.Name)
            .IsUnique(true);

        modelBuilder.Entity<TaskItem>()
            .HasIndex(x => new { x.ProjectId, x.Status });

        modelBuilder.Entity<TaskTag>()
            .HasKey(x => new { x.TaskItemId, x.TagId });

        modelBuilder.Entity<TaskTag>()
            .HasOne(x => x.TaskItem)
            .WithMany(t => t.TaskTags)
            .HasForeignKey(x => x.TaskItemId);

        modelBuilder.Entity<TaskTag>()
            .HasOne(x => x.Tag)
            .WithMany(t => t.TaskTags)
            .HasForeignKey(x => x.TagId);
    }
}