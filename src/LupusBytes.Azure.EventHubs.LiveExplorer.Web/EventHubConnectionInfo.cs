namespace LupusBytes.Azure.EventHubs.LiveExplorer.Web;

internal sealed record EventHubConnectionInfo(
    string ServiceKey,
    string ConnectionString,
    string Endpoint,
    string ConsumerGroup);