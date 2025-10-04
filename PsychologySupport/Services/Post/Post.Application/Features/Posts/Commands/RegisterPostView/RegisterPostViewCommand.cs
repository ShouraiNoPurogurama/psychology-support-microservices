using BuildingBlocks.CQRS;
using System;
using System.Collections.Generic;

namespace Post.Application.Features.Posts.Commands.RegisterPostView;

public record RegisterPostViewCommand(
    Guid IdempotencyKey,
    IReadOnlyList<Guid> PostIds 
) : IdempotentCommand<RegisterPostViewResult>(IdempotencyKey);

public record RegisterPostViewResult(
    int PostsUpdatedCount, 
    DateTimeOffset ViewedAt
);