namespace LupusBytes.Azure.EventHubs.LiveExplorer.Contracts;

public interface ILiveExplorerHub
{
    Task CreateMessage(string serviceKey, string message);

    Task JoinGroup(string serviceKey);
}