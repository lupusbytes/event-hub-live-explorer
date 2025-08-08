namespace LupusBytes.Azure.EventHubs.LiveExplorer.Contracts.SignalR;

public interface ILiveExplorerHub
{
    Task CreateMessage(string serviceKey, string message);

    Task JoinGroup(string serviceKey, string partitionId);

    Task LeaveGroup(string serviceKey, string partitionId);
}