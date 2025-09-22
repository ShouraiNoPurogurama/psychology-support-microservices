using BuildingBlocks.CQRS;
using Media.Application.Data;
using Media.Application.Exceptions;
using Media.Application.Features.Media.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Media.Application.Features.Media.Queries;

public record GetMediaQuery(Guid MediaId) : IQuery<GetMediaResult>;

public record GetMediaResult(MediaDto Media);

public class GetMediaHandler : IQueryHandler<GetMediaQuery, GetMediaResult>
{
    private readonly IMediaDbContext _context;

    public GetMediaHandler(IMediaDbContext context)
    {
        _context = context;
    }

    public async Task<GetMediaResult> Handle(GetMediaQuery request, CancellationToken cancellationToken)
    {
        var media = await _context.MediaAssets
            .AsNoTracking()
            .Include(m => m.Variants)
            .Where(m => m.Id == request.MediaId && !m.IsDeleted)
            .Select(m => new MediaDto(
                m.Id,
                m.State,
                m.Content.MimeType,
                m.Content.SizeInBytes,
                m.Checksum.Value,
                m.Variants.Select(v => new MediaVariantDto(
                    v.Id,
                    v.VariantType,
                    v.Format,
                    v.Width,
                    v.Height,
                    v.CdnUrl
                )).ToList()
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (media == null)
        {
            throw new MediaNotFoundException($"Media with ID {request.MediaId} not found.");
        }

        return new GetMediaResult(media);
    }
}