namespace LupusBytes.Azure.EventHubs.LiveExplorer.Contracts;

public record EventHubMessage(
    string PartitionId,
    long SequenceNumber,
    string Message);