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

    public async IAsyncEnumerable<EventHubMessage> JoinGroupAndGetMessages(string serviceKey)
    {
        var eventHubService = serviceProvider.GetEventHubService(serviceKey);
        var messagesCount = eventHubService.Messages.Count;

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            serviceKey,
            Context.ConnectionAborted);

        foreach (var message in eventHubService.Messages.Take(messagesCount))
        {
            yield return message;
        }
    }

    public Task LeaveGroup(string serviceKey)
        => Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            serviceKey,
            Context.ConnectionAborted);
}