namespace LupusBytes.Azure.EventHubs.LiveExplorer.Web.Contracts;

public record EventHubMessage(
    string PartitionId,
    long SequenceNumber,
    string Message);