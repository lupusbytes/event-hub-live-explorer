using LupusBytes.Azure.EventHubs.LiveExplorer.Contracts;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LupusBytes.Azure.EventHubs.LiveExplorer.Handlers;

internal class GetEventHubHandler(EventHubServiceProvider eventHubServiceProvider)
{
    public Results<Ok<EventHubInfo>, NotFound> Execute(string serviceKey)
        => eventHubServiceProvider.TryGetEventHubService(serviceKey, out var service)
            ? TypedResults.Ok(new EventHubInfo(service.Endpoint, service.ServiceKey, service.PartitionIds))
            : TypedResults.NotFound();
}