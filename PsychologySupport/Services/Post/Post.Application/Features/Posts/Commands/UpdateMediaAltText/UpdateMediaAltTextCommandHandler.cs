using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Microsoft.EntityFrameworkCore;
using Post.Application.Abstractions.Integration;
using Post.Application.Abstractions.Integration.Events;
using Post.Application.Data;

namespace Post.Application.Features.Posts.Commands.UpdateMediaAltText;

public sealed class UpdateMediaAltTextCommandHandler : ICommandHandler<UpdateMediaAltTextCommand, UpdateMediaAltTextResult>
{
    private readonly IPostDbContext _postDbContext;
    private readonly IOutboxWriter _outboxWriter;

    public UpdateMediaAltTextCommandHandler(IPostDbContext postDbContext, IOutboxWriter outboxWriter)
    {
        _postDbContext = postDbContext;
        _outboxWriter = outboxWriter;
    }

    public async Task<UpdateMediaAltTextResult> Handle(UpdateMediaAltTextCommand command, CancellationToken cancellationToken)
    {
        var media = await _postDbContext.PostMedia
            .FirstOrDefaultAsync(x => x.Id == command.MediaId && !x.IsDeleted, cancellationToken);
        if (media == null)
        {
            throw new NotFoundException($"PostMedia with Id {command.MediaId} not found.");
        }

        media.UpdateAltText(command.AltText);
        await _postDbContext.SaveChangesAsync(cancellationToken);

        var integrationEvent = new PostMediaAltTextUpdatedIntegrationEvent(media.PostId, media.Id, media.AltText);
        await _outboxWriter.WriteAsync(integrationEvent, cancellationToken);

        return new UpdateMediaAltTextResult(media.Id, media.AltText ?? string.Empty, DateTimeOffset.UtcNow);
    }
}