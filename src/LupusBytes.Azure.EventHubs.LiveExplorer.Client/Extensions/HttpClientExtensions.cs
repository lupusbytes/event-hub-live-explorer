using System.Globalization;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using LupusBytes.Azure.EventHubs.LiveExplorer.Contracts;

namespace LupusBytes.Azure.EventHubs.LiveExplorer.Client.Extensions;

internal static class HttpClientExtensions
{
    public static async Task<List<EventHubInfo>> GetEventHubsAsync(
        this HttpClient httpClient,
        CancellationToken cancellationToken = default)
    {
        var eventHubs = await httpClient.GetFromJsonAsync<List<EventHubInfo>>(
            "api/event-hubs",
            cancellationToken);

        return eventHubs!;
    }

    public static async Task<EventHubInfo> GetEventHubAsync(
        this HttpClient httpClient,
        string serviceKey,
        CancellationToken cancellationToken = default)
    {
        var eventHub = await httpClient.GetFromJsonAsync<EventHubInfo>(
            $"api/event-hubs/{serviceKey}",
            cancellationToken);

        return eventHub!;
    }

    public static async IAsyncEnumerable<EventHubMessage> GetEventHubPartitionMessagesAsync(
        this HttpClient httpClient,
        string serviceKey,
        string partitionId,
        long? fromSequenceNumber = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        string? continuationToken = null;
        do
        {
            var url = $"api/event-hubs/{serviceKey}/partitions/{partitionId}/events";

            if (continuationToken is not null)
            {
                url += $"?continuationToken={continuationToken}";
            }
            else if (fromSequenceNumber is not null)
            {
                url += "?fromSequenceNumber=" + fromSequenceNumber.Value.ToString(CultureInfo.InvariantCulture);
            }

            var messagesByPartitionId = await httpClient.GetFromJsonAsync<PagedResult<EventHubMessage>>(
                url,
                cancellationToken);

            foreach (var message in messagesByPartitionId!.Items)
            {
                yield return message;
            }

            continuationToken = messagesByPartitionId.ContinuationToken;
        }
        while (continuationToken is not null);
    }
}