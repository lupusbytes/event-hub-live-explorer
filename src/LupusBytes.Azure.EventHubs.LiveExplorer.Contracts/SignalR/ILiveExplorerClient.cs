namespace LupusBytes.Azure.EventHubs.LiveExplorer.Contracts.SignalR;

public interface ILiveExplorerClient
{
    Task LoadMessage(string eventHubNamespace, string name, EventHubMessage message);
}