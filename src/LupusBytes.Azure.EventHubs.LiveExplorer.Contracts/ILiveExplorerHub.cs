namespace LupusBytes.Azure.EventHubs.LiveExplorer.Contracts;

public interface ILiveExplorerHub
{
    Task CreateMessage(string serviceKey, string message);

    IAsyncEnumerable<EventHubMessage> JoinGroupAndGetMessages(string serviceKey);

    Task LeaveGroup(string serviceKey);
}