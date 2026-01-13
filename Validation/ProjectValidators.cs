using FluentValidation;
using TaskFlow.Api.DTOs;

namespace TaskFlow.Api.Validation;

public class CreateProjectRequestValidator : AbstractValidator<CreateProjectRequest>
{
    public CreateProjectRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().MaximumLength(120);
        RuleFor(x => x.Description)
            .MaximumLength(1000);
    }
}

public class UpdateProjectRequestValidator : AbstractValidator<UpdateProjectRequest>
{
    public UpdateProjectRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().MaximumLength(120);
        RuleFor(x => x.Description)
            .MaximumLength(1000);
    }
}