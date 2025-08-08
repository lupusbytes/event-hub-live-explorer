namespace LupusBytes.Azure.EventHubs.LiveExplorer.Contracts;

public record PagedResult<T>(string? ContinuationToken, IEnumerable<T> Items);