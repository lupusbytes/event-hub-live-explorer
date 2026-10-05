using LupusBytes.Azure.EventHubs.LiveExplorer.Contracts.SignalR;
using Microsoft.AspNetCore.SignalR;

namespace LupusBytes.Azure.EventHubs.LiveExplorer;

internal class LiveExplorerHub(EventHubServiceProvider serviceProvider) : Hub<ILiveExplorerClient>, ILiveExplorerHub
{
    public Task CreateMessage(string eventHubNamespace, string name, string message)
        => serviceProvider
        .GetEventHubService(eventHubNamespace, name)
        .SendEventAsync(
            message,
            Context.ConnectionAborted);

    public Task JoinGroup(
        string eventHubNamespace,
        string name,
        string partitionId)
        => Groups.AddToGroupAsync(
            Context.ConnectionId,
            GetGroupName(eventHubNamespace, name, partitionId),
            Context.ConnectionAborted);

    public Task LeaveGroup(string eventHubNamespace, string name, string partitionId)
        => Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            GetGroupName(eventHubNamespace, name, partitionId),
            Context.ConnectionAborted);

    internal static string GetGroupName(string eventHubNamespace, string name, string partitionId)
        => $"{eventHubNamespace}/{name}/{partitionId}";
}