namespace EventGenerator;

internal sealed class HumidityEventHubProducer([FromKeyedServices("humidity")] EventHubProducerClient producerClient)
    : EventHubProducerBackgroundService(producerClient, GenerateHumidityData, TimeSpan.FromSeconds(5))
{
    private static object GenerateHumidityData()
    {
        var humidity = (Random.Shared.NextDouble() * 5) + 25; // Generates between 25% and 30%

        return new
        {
            type = "humidity",
            value = Math.Round(humidity, 2),
            unit = "%",
            timestamp = DateTime.UtcNow,
        };
    }
}