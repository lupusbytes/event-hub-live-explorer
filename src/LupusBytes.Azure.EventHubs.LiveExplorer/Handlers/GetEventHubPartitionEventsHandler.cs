using System.Globalization;
using LupusBytes.Azure.EventHubs.LiveExplorer.Contracts;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LupusBytes.Azure.EventHubs.LiveExplorer.Handlers;

internal class GetEventHubPartitionEventsHandler(EventHubServiceProvider eventHubServiceProvider)
{
    public Results<Ok<PagedResult<EventHubMessage>>, NotFound> Execute(
        string serviceKey,
        string partitionId,
        long? fromSequenceNumber,
        string? continuationToken)
    {
        const int maxEventsPerRequest = 50;

        if (!eventHubServiceProvider.TryGetEventHubService(serviceKey, out var eventHubService) ||
            !eventHubService.TryGetEventsFromPartition(partitionId, out var partitionEvents))
        {
            return TypedResults.NotFound();
        }

        var offset = 0;
        if (!string.IsNullOrEmpty(continuationToken))
        {
            _ = int.TryParse(continuationToken, CultureInfo.InvariantCulture, out offset);
        }
        else if (fromSequenceNumber is not null && TryGetIndexOfSequenceNumber(partitionEvents, fromSequenceNumber.Value, out var index))
        {
            offset = index + 1;
        }

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

    private static bool TryGetIndexOfSequenceNumber(List<EventHubMessage> messages, long sequenceNumber, out int index)
    {
        var subject = new EventHubMessage(string.Empty, sequenceNumber, DateTimeOffset.MinValue, string.Empty);
        var comparer = Comparer<EventHubMessage>.Create((x, y) => x.SequenceNumber.CompareTo(y.SequenceNumber));
        index = messages.BinarySearch(subject, comparer);
        return index >= 0;
    }
}