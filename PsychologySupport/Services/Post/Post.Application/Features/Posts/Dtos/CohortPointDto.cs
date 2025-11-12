namespace Post.Application.Features.Posts.Dtos;

public sealed record CohortPointDto(
    int Week,
    int Active,
    double Percent
);