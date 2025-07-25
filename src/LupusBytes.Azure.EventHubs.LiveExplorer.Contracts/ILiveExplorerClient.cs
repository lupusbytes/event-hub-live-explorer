namespace LupusBytes.Azure.EventHubs.LiveExplorer.Contracts;

public interface ILiveExplorerClient
{
    Task LoadMessage(EventHubMessage message);
}