using FluentValidation;
using TaskFlow.Api.DTOs;

namespace TaskFlow.Api.Validation;

public class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
{
    public CreateTaskRequestValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(4000);
        RuleFor(x => x.Tags)
            .Must(tags => tags == null || tags.All(t => !string.IsNullOrWhiteSpace(t)))
            .WithMessage("Tags must not contain empty values.");
        RuleForEach(x => x.Tags)
            .MaximumLength(40);
    }
}

public class UpdateTaskRequestValidator : AbstractValidator<UpdateTaskRequest>
{
    public UpdateTaskRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(4000);
    }
}

public class AddCommentRequestValidator : AbstractValidator<AddCommentRequest>
{
    public AddCommentRequestValidator()
    {
        RuleFor(x => x.Text).NotEmpty().MaximumLength(2000);
    }
}