namespace LupusBytes.Azure.EventHubs.LiveExplorer.Contracts;

public interface ILiveExplorerHub
{
    Task ReadEvent(EventHubMessage message);

    Task WriteEvent(string message);

    Task JoinGroup(string groupName);
}