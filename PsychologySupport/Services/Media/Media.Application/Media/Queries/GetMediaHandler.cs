using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Media.Application.Data;
using Media.Application.Dtos;
using Media.Application.Exceptions;
using Microsoft.EntityFrameworkCore;


namespace Media.Application.Media.Queries;

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
            .Include(m => m.MediaVariants) 
            .Where(m => m.Id == request.MediaId && m.ExifRemoved == false)
            .Select(m => new MediaDto(
                m.Id,
                m.State,
                m.SourceMime,
                m.SourceBytes,
                m.ChecksumSha256,
                m.MediaVariants.Select(v => new MediaVariantDto(
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