namespace LupusBytes.Azure.EventHubs.LiveExplorer.Web.Extensions;

internal static class ConfigurationExtensions
{
    public static IEnumerable<EventHubConnectionInfo> GetEventHubConnections(this IConfiguration config)
    {
        foreach (var connectionString in config.GetSection("ConnectionStrings").GetChildren())
        {
            if (connectionString.Value is null)
            {
                continue;
            }

            var properties = connectionString.Value
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(part => part.Split('=', 2))
                .ToDictionary(kv => kv[0], kv => kv[1], StringComparer.OrdinalIgnoreCase);

            // Ensure the current ConnectionString is for an EventHub
            if (!properties.TryGetValue("Endpoint", out var endpoint) ||
                !endpoint.StartsWith("sb://", StringComparison.Ordinal))
            {
                continue;
            }

            var consumerGroup = properties.GetValueOrDefault("ConsumerGroup", "$Default");

            yield return new EventHubConnectionInfo(
                connectionString.Key,
                connectionString.Value,
                endpoint,
                consumerGroup);
        }
    }
}