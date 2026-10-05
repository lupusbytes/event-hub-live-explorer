namespace LupusBytes.Azure.EventHubs.LiveExplorer.Contracts;

public record EventHubInfo(
    string Endpoint,
    string Namespace,
    string Name,
    IReadOnlyCollection<string> PartitionIds);