namespace TaskFlow.Api.DTOs;

public record AddCommentRequest(string Text);

public record CommentResponse(
    Guid Id,
    Guid TaskItemId,
    string Text,
    DateTime CreatedAtUtc
);