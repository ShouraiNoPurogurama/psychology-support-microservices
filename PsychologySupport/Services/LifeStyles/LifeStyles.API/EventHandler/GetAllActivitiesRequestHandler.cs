using BuildingBlocks.Messaging.Events.Queries.LifeStyle;
using LifeStyles.API.Features.ActivitiesCommon;
using MassTransit;
using MediatR;

namespace LifeStyles.API.EventHandler;

public class GetAllActivitiesRequestHandler(ISender sender) : IConsumer<GetAllActivitiesRequest>
{
    public async Task Consume(ConsumeContext<GetAllActivitiesRequest> context)
    {
        var query = new GetAllActivitiesQuery();
        
        var result = await sender.Send(query, context.CancellationToken);
        
        var response = new GetAllActivitiesResponse(result.Activities.ToList());
        
        await context.RespondAsync(response);
    }
}