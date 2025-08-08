namespace LupusBytes.Azure.EventHubs.LiveExplorer.Contracts;

public record EventHubInfo(
    string Endpoint,
    string ServiceKey,
    IReadOnlyCollection<string> PartitionIds);