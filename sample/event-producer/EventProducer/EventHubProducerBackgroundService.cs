namespace EventGenerator;

internal abstract class EventHubProducerBackgroundService(
    EventHubProducerClient eventHubProducerClient,
    Func<object> messageFactory,
    TimeSpan delayBetweenMessages) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var message = messageFactory.Invoke();
            var json = JsonSerializer.Serialize(message);
            var eventData = new EventData(Encoding.UTF8.GetBytes(json));

            try
            {
                await eventHubProducerClient.SendAsync([eventData], cancellationToken: stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                Console.WriteLine($"Failed to send event hub message: {ex.Message}");
            }

            await Task.Delay(delayBetweenMessages, stoppingToken);
        }
    }
}