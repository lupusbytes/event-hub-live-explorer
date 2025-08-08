using LupusBytes.Azure.EventHubs.LiveExplorer.Contracts;
using LupusBytes.Azure.EventHubs.LiveExplorer.Contracts.SignalR;
using Microsoft.AspNetCore.SignalR;

namespace LupusBytes.Azure.EventHubs.LiveExplorer;

internal class LiveExplorerHub(EventHubServiceProvider serviceProvider) : Hub<ILiveExplorerClient>, ILiveExplorerHub
{
    public Task CreateMessage(string serviceKey, string message)
        => serviceProvider
        .GetEventHubService(serviceKey)
        .SendEventAsync(
            message,
            Context.ConnectionAborted);

    public Task JoinGroup(
        string serviceKey,
        string partitionId)
        => Groups.AddToGroupAsync(
            Context.ConnectionId,
            GetGroupName(serviceKey, partitionId),
            Context.ConnectionAborted);

    public Task LeaveGroup(string serviceKey, string partitionId)
        => Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            GetGroupName(serviceKey, partitionId),
            Context.ConnectionAborted);

    private static string GetGroupName(string serviceKey, string partitionId)
        => $"{serviceKey}-{partitionId}";
}