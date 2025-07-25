namespace LupusBytes.Azure.EventHubs.LiveExplorer;

internal sealed record EventHubConnectionInfo(
    string ServiceKey,
    string ConnectionString,
    string Endpoint,
    string ConsumerGroup);