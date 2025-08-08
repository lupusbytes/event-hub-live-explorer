using System.Globalization;
using LupusBytes.Azure.EventHubs.LiveExplorer.Contracts;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LupusBytes.Azure.EventHubs.LiveExplorer.Handlers;

internal class GetEventHubPartitionEventsHandler(EventHubServiceProvider eventHubServiceProvider)
{
    public Results<Ok<PagedResult<EventHubMessage>>, NotFound> Execute(
        string serviceKey,
        string partitionId,
        string? continuationToken)
    {
        const int maxEventsPerRequest = 50;

        if (!eventHubServiceProvider.TryGetEventHubService(serviceKey, out var eventHubService) ||
            !eventHubService.TryGetEventsFromPartition(partitionId, out var partitionEvents))
        {
            return TypedResults.NotFound();
        }

        _ = int.TryParse(continuationToken, CultureInfo.InvariantCulture, out var offset);

        var pagedEvents = partitionEvents
            .Skip(offset)
            .Take(maxEventsPerRequest)
            .ToList();

        // Only set continuation token if we have events and haven't reached the end
        string? nextContinuationToken = null;
        var nextOffset = offset + pagedEvents.Count;
        if (nextOffset < partitionEvents.Count)
        {
            nextContinuationToken = nextOffset.ToString(CultureInfo.InvariantCulture);
        }

        return TypedResults.Ok(new PagedResult<EventHubMessage>(nextContinuationToken, pagedEvents));
    }
}