using System.Diagnostics.CodeAnalysis;
using LupusBytes.Azure.EventHubs.LiveExplorer.Client.Extensions;
using LupusBytes.Azure.EventHubs.LiveExplorer.Contracts;
using LupusBytes.Azure.EventHubs.LiveExplorer.Contracts.SignalR;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor;
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
    private Dictionary<string, long>? latestSequenceNumberByPartitionId;

    private CancellationTokenSource? processParametersCts;
    private Task? processParametersTask;

    [Parameter]
    public string ServiceKey { get; set; } = string.Empty;

    private string? lastServiceKey;

    private IReadOnlyCollection<string>? lastPartitionIds;

    private string input = string.Empty;

    private bool isValidInput;

    private bool isPlaying = true;

    private string CurrentIcon => isPlaying ? Icons.Material.Filled.Pause : Icons.Material.Filled.PlayArrow;

    public EventHub(HttpClient httpClient)
    {
        this.httpClient = httpClient;
        connection = new HubConnectionBuilder().WithUrl(httpClient.BaseAddress + "notifications").Build();
        hub = connection.CreateHubProxy<ILiveExplorerHub>(CancellationToken.None);
        subscription = connection.Register<ILiveExplorerClient>(this);
    }

    protected override Task OnInitializedAsync()
        => connection.StartAsync(CancellationToken.None);

    protected override async Task OnParametersSetAsync()
    {
        var newCts = new CancellationTokenSource();
        var oldCts = Interlocked.Exchange(ref processParametersCts, newCts);
        var oldTask = processParametersTask;

        if (oldCts is not null)
        {
            await oldCts.CancelAsync();

            if (oldTask is not null)
            {
                try
                {
                    await oldTask;
                }
                catch (OperationCanceledException)
                {
                    // Ignore
                }
            }

            oldCts.Dispose();
        }

        processParametersTask = ProcessParametersAsync(newCts.Token);
    }

    private async Task ProcessParametersAsync(CancellationToken cancellationToken)
    {
        if (lastServiceKey is not null && lastPartitionIds is not null)
        {
            foreach (var partitionId in lastPartitionIds)
            {
                await hub.LeaveGroup(lastServiceKey, partitionId);
            }

            messages.Clear();
        }

        isPlaying = true;

        eventHub = await httpClient.GetEventHubAsync(ServiceKey, cancellationToken);
        lastServiceKey = ServiceKey;
        lastPartitionIds = eventHub!.PartitionIds;

        foreach (var partitionId in eventHub.PartitionIds)
        {
            await foreach (var message in httpClient.GetEventHubPartitionMessagesAsync(
                               ServiceKey,
                               partitionId,
                               cancellationToken: cancellationToken))
            {
                messages.Add(message);

                if (messages.Count % 50 == 0)
                {
                    await InvokeAsync(StateHasChanged);
                }
            }

            cancellationToken.ThrowIfCancellationRequested();

            await hub.JoinGroup(ServiceKey, partitionId);
        }

        await InvokeAsync(StateHasChanged);
    }

    public Task LoadMessage(string serviceKey, EventHubMessage message)
    {
        if (serviceKey != ServiceKey)
        {
            return Task.CompletedTask;
        }

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

    private void ClearMessages()
    {
        latestSequenceNumberByPartitionId = lastPartitionIds?
            .ToDictionary(
                partitionId => partitionId,
                FindLatestSequenceNumberByPartitionId,
                StringComparer.OrdinalIgnoreCase);

        messages.Clear();
    }

    private async Task TogglePlayPause()
    {
        isPlaying = !isPlaying;
        var cancellationToken = processParametersCts?.Token ?? CancellationToken.None;

        foreach (var partitionId in eventHub!.PartitionIds)
        {
            if (isPlaying)
            {
                await foreach (var message in httpClient.GetEventHubPartitionMessagesAsync(
                                   ServiceKey,
                                   partitionId,
                                   latestSequenceNumberByPartitionId?[partitionId],
                                   cancellationToken))
                {
                    messages.Add(message);

                    if (messages.Count % 50 == 0)
                    {
                        await InvokeAsync(StateHasChanged);
                    }
                }

                await hub.JoinGroup(ServiceKey, partitionId);
            }
            else
            {
                await hub.LeaveGroup(ServiceKey, partitionId);
                latestSequenceNumberByPartitionId ??= new Dictionary<string, long>(StringComparer.Ordinal);
                latestSequenceNumberByPartitionId[partitionId] = FindLatestSequenceNumberByPartitionId(partitionId);
            }
        }
    }

    private long FindLatestSequenceNumberByPartitionId(string partitionId)
    {
        // The messages list is sorted by sequence numbers ascending, so to efficiently find the latest sequence number
        // we loop the list backwards and return the first sequence number that matches the partition id
        for (var i = messages.Count - 1; i >= 0; i--)
        {
            if (messages[i].PartitionId == partitionId)
            {
                return messages[i].SequenceNumber;
            }
        }

        return 0L;
    }

    public ValueTask DisposeAsync()
    {
        subscription?.Dispose();
        processParametersCts?.Dispose();
        return connection.DisposeAsync();
    }
}