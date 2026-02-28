namespace LupusBytes.Azure.EventHubs.LiveExplorer.Contracts;

public record EventHubMessage(
    string PartitionId,
    long SequenceNumber,
    DateTimeOffset EnqueuedTime,
    string Message,
    string? ContentType = null,
    string? CorrelationId = null,
    string? MessageId = null,
    IReadOnlyDictionary<string, object>? Properties = null);