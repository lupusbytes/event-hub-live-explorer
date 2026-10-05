namespace LupusBytes.Azure.EventHubs.LiveExplorer;

internal sealed record EventHubConnectionInfo(
    string Namespace,
    string Name,
    string ConnectionString,
    string Endpoint,
    string ConsumerGroup)
{
    public string Id => $"{Namespace}/{Name}";
}