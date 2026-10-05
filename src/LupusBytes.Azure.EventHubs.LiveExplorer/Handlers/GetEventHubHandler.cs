using LupusBytes.Azure.EventHubs.LiveExplorer.Contracts;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LupusBytes.Azure.EventHubs.LiveExplorer.Handlers;

internal class GetEventHubHandler(EventHubServiceProvider eventHubServiceProvider)
{
    public Results<Ok<EventHubInfo>, NotFound> Execute(string eventHubNamespace, string name)
        => eventHubServiceProvider.TryGetEventHubService(eventHubNamespace, name, out var service)
            ? TypedResults.Ok(new EventHubInfo(service.Endpoint, service.Namespace, service.Name, service.PartitionIds))
            : TypedResults.NotFound();
}