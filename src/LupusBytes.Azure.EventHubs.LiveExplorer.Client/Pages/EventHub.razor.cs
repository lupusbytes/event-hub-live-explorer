using System.Diagnostics.CodeAnalysis;
using LupusBytes.Azure.EventHubs.LiveExplorer.Client.Extensions;
using LupusBytes.Azure.EventHubs.LiveExplorer.Contracts;
using LupusBytes.Azure.EventHubs.LiveExplorer.Contracts.SignalR;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using TypedSignalR.Client;

namespace LupusBytes.Azure.EventHubs.LiveExplorer.Client.Pages;

[SuppressMessage("Maintainability", "CA1515:Consider making public types internal",  Justification = "Impossible")]
public sealed partial class EventHub : ComponentBase, ILiveExplorerClient, IAsyncDisposable
{
    private readonly List<EventHubMessage> messages = [];
    private readonly HubConnection connection;
    private readonly ILiveExplorerHub hub;
    private readonly IDisposable? subscription;
    private readonly HttpClient httpClient;
    private EventHubInfo? eventHub;

    [Parameter]
    public string ServiceKey { get; set; } = string.Empty;

    private string? lastServiceKey;

    private IReadOnlyCollection<string>? lastPartitionIds;

    private string input = string.Empty;

    private bool isValidInput;

    public EventHub(HttpClient httpClient)
    {
        this.httpClient = httpClient;
        connection = new HubConnectionBuilder().WithUrl(httpClient.BaseAddress + "notifications").Build();
        hub = connection.CreateHubProxy<ILiveExplorerHub>();
        subscription = connection.Register<ILiveExplorerClient>(this);
    }

    protected override async Task OnParametersSetAsync()
    {
        eventHub = await httpClient.GetEventHubAsync(ServiceKey);

        if (connection is not { State: HubConnectionState.Connected or HubConnectionState.Connecting })
        {
            await connection.StartAsync();
        }

        if (lastServiceKey is not null && lastPartitionIds is not null)
        {
            foreach (var partitionId in lastPartitionIds)
            {
                await hub.LeaveGroup(lastServiceKey, partitionId);
            }

            messages.Clear();
        }

        lastServiceKey = ServiceKey;
        lastPartitionIds = eventHub!.PartitionIds;

        foreach (var partitionId in eventHub.PartitionIds)
        {
            await foreach (var message in httpClient.GetEventHubPartitionMessagesAsync(ServiceKey, partitionId))
            {
                messages.Add(message);

                if (messages.Count % 50 == 0)
                {
                    await InvokeAsync(StateHasChanged);
                }
            }

            await hub.JoinGroup(ServiceKey, partitionId);
        }

        await InvokeAsync(StateHasChanged);
    }

    public Task LoadMessage(EventHubMessage message)
    {
        messages.Add(message);
        return InvokeAsync(StateHasChanged);
    }

    private async Task SubmitAsync()
    {
        if (!string.IsNullOrWhiteSpace(input))
        {
            await hub.CreateMessage(ServiceKey, input);
            input = string.Empty;
        }
    }

    public ValueTask DisposeAsync()
    {
        subscription?.Dispose();
        return connection.DisposeAsync();
    }
}