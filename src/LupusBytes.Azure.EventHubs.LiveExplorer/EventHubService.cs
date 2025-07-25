using System.Diagnostics.CodeAnalysis;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Producer;
using LupusBytes.Azure.EventHubs.LiveExplorer.Contracts;
using Microsoft.AspNetCore.SignalR;

namespace LupusBytes.Azure.EventHubs.LiveExplorer;

internal partial class EventHubService(
    string serviceKey,
    EventHubConsumerClient consumer,
    EventHubProducerClient producer,
    IHubContext<LiveExplorerHub, ILiveExplorerClient> hubContext,
    ILogger<EventHubService> logger)
    : BackgroundService
{
    private readonly List<EventHubMessage> messages = [];

    public IReadOnlyCollection<EventHubMessage> Messages => messages;

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "We never want to crash here")]
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
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
                @event.Data.EventBody.ToString());

            messages.Add(message);
            await hubContext.Clients.Groups(serviceKey).LoadMessage(message);
        }
    }

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