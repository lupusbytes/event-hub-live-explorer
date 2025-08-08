using LupusBytes.Azure.EventHubs.LiveExplorer.Contracts;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LupusBytes.Azure.EventHubs.LiveExplorer.Handlers;

internal class GetEventHubsHandler(EventHubServiceProvider eventHubServiceProvider)
{
    public Ok<IEnumerable<EventHubInfo>> Execute() => TypedResults.Ok(
        eventHubServiceProvider
            .GetEventHubServices()
            .Select(x => new EventHubInfo(x.Endpoint, x.ServiceKey, x.PartitionIds)));
}