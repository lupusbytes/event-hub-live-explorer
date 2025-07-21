using LupusBytes.Azure.EventHubs.LiveExplorer.Web.Contracts;
using Microsoft.AspNetCore.SignalR;

namespace LupusBytes.Azure.EventHubs.LiveExplorer.Web;

internal class LiveExplorerHub(EventHubServiceProvider serviceProvider) : Hub<ILiveExplorerHub>
{
    public Task WriteEvent(string eventHub, string message)
        => serviceProvider
            .GetEventHubService(eventHub)
            .SendEventAsync(
                message,
                Context.ConnectionAborted);

    public Task JoinGroup(string groupName)
        => Groups.AddToGroupAsync(
            Context.ConnectionId,
            groupName,
            Context.ConnectionAborted);
}