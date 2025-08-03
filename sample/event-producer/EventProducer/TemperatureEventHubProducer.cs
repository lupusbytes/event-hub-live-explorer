namespace EventGenerator;

internal sealed class TemperatureEventHubProducer([FromKeyedServices("temperature")] EventHubProducerClient producerClient)
    : EventHubProducerBackgroundService(producerClient, GenerateTemperatureData, TimeSpan.FromSeconds(10))
{
    private static object GenerateTemperatureData()
    {
        var temperature = (Random.Shared.NextDouble() * 2) + 20; // Generates between 20 and 22

        return new
        {
            type = "temperature",
            value = Math.Round(temperature, 2),
            unit = "C",
            timestamp = DateTime.UtcNow,
        };
    }
}