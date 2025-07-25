using LupusBytes.Azure.EventHubs.LiveExplorer.Contracts;
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

    public Task JoinGroup(string serviceKey)
        => Groups.AddToGroupAsync(
            Context.ConnectionId,
            serviceKey,
            Context.ConnectionAborted);
}