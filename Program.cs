using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Data;
using TaskFlow.Api.Middleware;
using TaskFlow.Api.Services;
using TaskFlow.Api.Validation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(o =>
    {
        // Чтобы валидация тоже возвращала ProblemDetails
        o.InvalidModelStateResponseFactory = ctx =>
        {
            var problem = new ValidationProblemDetails(ctx.ModelState)
            {
                Title = "Validation error",
                Status = StatusCodes.Status400BadRequest,
                Type = "https://httpstatuses.com/400"
            };
            return new BadRequestObjectResult(problem)
            {
                ContentTypes = { "application/problem+json" }
            };
        };
    });

builder.Services.AddEndpointsApiExplorer();

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateProjectRequestValidator>();

// Db
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    var cs = builder.Configuration.GetConnectionString("DefaultConnection");
    opt.UseNpgsql(cs);
});

// Services
builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<TaskService>();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();


app.MapControllers();
app.Run();