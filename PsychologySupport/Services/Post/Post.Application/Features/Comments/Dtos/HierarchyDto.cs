namespace Post.Application.Features.Comments.Dtos;

public record HierarchyDto(
    Guid? ParentCommentId,
    string Path,
    int Level
);
