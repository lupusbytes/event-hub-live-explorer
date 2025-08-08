using System.Diagnostics.CodeAnalysis;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Producer;
using LupusBytes.Azure.EventHubs.LiveExplorer.Contracts;
using LupusBytes.Azure.EventHubs.LiveExplorer.Contracts.SignalR;
using Microsoft.AspNetCore.SignalR;
using Polly;

namespace LupusBytes.Azure.EventHubs.LiveExplorer;

internal partial class EventHubService(
    string serviceKey,
    string endpoint,
    EventHubConsumerClient consumer,
    EventHubProducerClient producer,
    IHubContext<LiveExplorerHub, ILiveExplorerClient> hubContext,
    ILogger<EventHubService> logger)
    : BackgroundService
{
    private Dictionary<string, List<EventHubMessage>> partitions = [];

    public string ServiceKey => serviceKey;

    public string Endpoint => endpoint;

    public IReadOnlyCollection<string> PartitionIds => partitions.Keys;

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "We never want to crash here")]
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var partitionIds = await GetPartitionIds(stoppingToken);
        partitions = partitionIds.ToDictionary(id => id, _ => new List<EventHubMessage>(), StringComparer.Ordinal);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ReadAndProcessEventsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                LogExecutionException(ex, serviceKey);
            }
        }
    }

    private async Task ReadAndProcessEventsAsync(CancellationToken stoppingToken)
    {
        await foreach (var @event in consumer.ReadEventsAsync(stoppingToken))
        {
            var message = new EventHubMessage(
                @event.Partition.PartitionId,
                @event.Data.SequenceNumber,
                @event.Data.EnqueuedTime,
                @event.Data.EventBody.ToString());

            partitions[@event.Partition.PartitionId].Add(message);
            await hubContext.Clients.Groups($"{serviceKey}-{@event.Partition.PartitionId}").LoadMessage(message);
        }
    }

    private Task<string[]> GetPartitionIds(CancellationToken cancellationToken) => Policy
        .Handle<Exception>()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2.5, attempt)))
        .ExecuteAsync(consumer.GetPartitionIdsAsync, cancellationToken);

    public bool TryGetEventsFromPartition(string partitionId, [NotNullWhen(true)] out List<EventHubMessage>? eventHubMessages)
        => partitions.TryGetValue(partitionId, out eventHubMessages);

    public async Task SendEventAsync(string message, CancellationToken cancellationToken = default)
    {
        using var eventBatch = await producer.CreateBatchAsync(cancellationToken);
        eventBatch.TryAdd(new EventData(message));
        await producer.SendAsync(eventBatch, cancellationToken);
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Error,
        Message = "Exception occured while reading and processing events from EventHub with key: {ServiceKey}")]
    private partial void LogExecutionException(Exception ex, string serviceKey);
}