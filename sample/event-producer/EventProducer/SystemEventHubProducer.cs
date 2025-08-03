namespace EventGenerator;

internal sealed class SystemEventHubProducer([FromKeyedServices("system-events")] EventHubProducerClient producerClient)
    : EventHubProducerBackgroundService(producerClient, GenerateSystemEvent, TimeSpan.FromSeconds(30))
{
    private static object GenerateSystemEvent()
        => new
        {
            eventType = "SystemHealthCheck",
            status = "Healthy",
            latencyMs = Random.Shared.Next(2, 200),
            timestamp = DateTimeOffset.UtcNow,
        };
}