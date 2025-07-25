using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using LupusBytes.Azure.EventHubs.LiveExplorer.Contracts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using TypedSignalR.Client;

namespace LupusBytes.Azure.EventHubs.LiveExplorer.Client.Pages;

[SuppressMessage("Maintainability", "CA1515:Consider making public types internal",  Justification = "Impossible")]
public sealed partial class EventHub(HttpClient httpClient) : ComponentBase, ILiveExplorerClient, IAsyncDisposable
{
    [Parameter]
    public string ServiceKey { get; set; } = string.Empty;

    private ILiveExplorerHub hub = null!;

    private HubConnection? connection;

    private List<EventHubMessage> messages = [];

    private string input = string.Empty;

    private bool isValidInput;

    private IDisposable? subscription;

    protected override Task OnParametersSetAsync()
        => InitializeDataAsync();

    private async Task InitializeDataAsync()
    {
        // Dispose previous connection if it exists
        await DisposeAsync();

        messages = await httpClient.GetFromJsonAsync<List<EventHubMessage>>($"api/event-hubs/{ServiceKey}/messages") ?? [];

        connection = new HubConnectionBuilder().WithUrl(httpClient.BaseAddress + "notifications").Build();
        hub = connection.CreateHubProxy<ILiveExplorerHub>();
        subscription = connection.Register<ILiveExplorerClient>(this);

        await connection.StartAsync();
        await hub.JoinGroup(ServiceKey);
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

    public async ValueTask DisposeAsync()
    {
        subscription?.Dispose();

        if (connection is not null)
        {
            await connection.DisposeAsync();
        }
    }
}