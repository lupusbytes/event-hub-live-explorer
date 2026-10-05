namespace LupusBytes.Azure.EventHubs.LiveExplorer.Extensions;

internal static class ConfigurationExtensions
{
    public static IEnumerable<EventHubConnectionInfo> GetEventHubConnections(this IConfiguration config)
    {
        var eventHubs = new HashSet<string>(StringComparer.Ordinal);

        var connectionStrings = config
            .GetSection("ConnectionStrings")
            .GetChildren()
            .Select(x => x.Value)
            .OfType<string>();

        foreach (var connectionString in connectionStrings)
        {
            var properties = connectionString
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(part => part.Split('=', 2))
                .Where(kv => kv.Length == 2)
                .ToDictionary(kv => kv[0], kv => kv[1], StringComparer.OrdinalIgnoreCase);

            // Ensure the current ConnectionString is for an EventHub
            if (!properties.TryGetValue("Endpoint", out var endpoint) ||
                !endpoint.StartsWith("sb://", StringComparison.Ordinal) ||
                !Uri.TryCreate(endpoint, UriKind.Absolute, out var endpointUri) ||
                !properties.TryGetValue("EntityPath", out var name) ||
                string.IsNullOrEmpty(name))
            {
                continue;
            }

            var eventHub = new EventHubConnectionInfo(
                endpointUri.Authority,
                name,
                connectionString,
                endpoint,
                properties.GetValueOrDefault("ConsumerGroup", "$Default"));

            if (eventHubs.Add(eventHub.Id))
            {
                yield return eventHub;
            }
        }
    }
}