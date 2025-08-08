namespace LupusBytes.Azure.EventHubs.LiveExplorer.Contracts.SignalR;

public interface ILiveExplorerClient
{
    Task LoadMessage(EventHubMessage message);
}