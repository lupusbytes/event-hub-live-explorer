using System.Diagnostics.CodeAnalysis;
using LupusBytes.Azure.EventHubs.LiveExplorer.Contracts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using TypedSignalR.Client;

namespace LupusBytes.Azure.EventHubs.LiveExplorer.Client.Pages;

[SuppressMessage("Maintainability", "CA1515:Consider making public types internal",  Justification = "Impossible")]
public sealed partial class EventHub : ComponentBase, ILiveExplorerClient, IAsyncDisposable
{
    private readonly HubConnection connection;
    private readonly ILiveExplorerHub hub;
    private readonly IDisposable? subscription;
    private readonly List<EventHubMessage> messages = [];

    [Parameter]
    public string ServiceKey { get; set; } = string.Empty;

    private string? lastServiceKey;

    private string input = string.Empty;

    private bool isValidInput;

    public EventHub(HttpClient httpClient)
    {
        connection = new HubConnectionBuilder().WithUrl(httpClient.BaseAddress + "notifications").Build();
        hub = connection.CreateHubProxy<ILiveExplorerHub>();
        subscription = connection.Register<ILiveExplorerClient>(this);
    }

    protected override async Task OnParametersSetAsync()
    {
        if (connection is not { State: HubConnectionState.Connected or HubConnectionState.Connecting })
        {
            await connection.StartAsync();
        }

        if (lastServiceKey is not null)
        {
            messages.Clear();
            await hub.LeaveGroup(lastServiceKey);
        }

        lastServiceKey = ServiceKey;

        await foreach (var message in hub.JoinGroupAndGetMessages(ServiceKey))
        {
            messages.Add(message);
        }
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