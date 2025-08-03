namespace EventGenerator;

internal sealed class UserEventHubProducer([FromKeyedServices("user-events")] EventHubProducerClient producerClient)
    : EventHubProducerBackgroundService(producerClient, GenerateUserEvent, TimeSpan.FromSeconds(3))
{
    private static readonly string[] Devices = ["Chrome on Windows", "Safari on iOS", "Edge on macOS", "Firefox on Linux"];

    private static object GenerateUserEvent()
    {
        var userId = $"user-{Random.Shared.Next(100, 999)}";
        var now = DateTimeOffset.UtcNow;

        return Random.Shared.Next(3) switch
        {
            0 => new
            {
                eventType = "UserLogin",
                timestamp = now,
                userId,
                ipAddress = $"192.168.1.{Random.Shared.Next(1, 255)}",
                device = Devices[Random.Shared.Next(Devices.Length)],
            },
            1 => new
            {
                eventType = "UserPostCreated",
                timestamp = now,
                userId,
                postId = $"post-{Guid.NewGuid()}",
            },
            2 => new
            {
                eventType = "UserProfileUpdated",
                timestamp = now,
                userId,
                changes = new
                {
                    email = $"user{Random.Shared.Next(1000)}@example.com",
                    displayName = userId,
                },
            },
            _ => throw new UnreachableException(),
        };
    }
}