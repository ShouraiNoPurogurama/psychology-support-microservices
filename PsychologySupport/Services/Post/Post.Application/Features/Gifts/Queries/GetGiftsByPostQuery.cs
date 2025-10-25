using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Post.Application.Features.Gifts.Dtos;

namespace Post.Application.Features.Gifts.Queries;

public record GetGiftsByPostQuery(
    Guid PostId,
    int Page = 1,
    int PageSize = 20,
    string SortBy = "CreatedAt",
    bool SortDescending = false) : IQuery<GetGiftsByPostResult>;
    
public record GetGiftsByPostResult(PaginatedResult<GiftAttachDto> Gifts);