namespace LupusBytes.Azure.EventHubs.LiveExplorer.Contracts.SignalR;

public interface ILiveExplorerHub
{
    Task CreateMessage(string eventHubNamespace, string name, string message);

    Task JoinGroup(string eventHubNamespace, string name, string partitionId);

    Task LeaveGroup(string eventHubNamespace, string name, string partitionId);
}