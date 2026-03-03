using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LupusBytes.Azure.EventHubs.LiveExplorer.Client.Extensions;
using LupusBytes.Azure.EventHubs.LiveExplorer.Contracts;
using LupusBytes.Azure.EventHubs.LiveExplorer.Contracts.SignalR;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using MudBlazor;
using TypedSignalR.Client;

namespace LupusBytes.Azure.EventHubs.LiveExplorer.Client.Pages;

[SuppressMessage("Maintainability", "CA1515:Consider making public types internal",  Justification = "Impossible")]
public sealed partial class EventHub : ComponentBase, ILiveExplorerClient, IAsyncDisposable
{
    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    [Inject]
    private IJSRuntime JS { get; set; } = null!;

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

    private bool isPlaying = true;

    private string searchFilter = string.Empty;

    private string? partitionFilter;

    private IEnumerable<EventHubMessage> FilteredMessages => messages
        .Where(m => string.IsNullOrEmpty(partitionFilter) || m.PartitionId == partitionFilter)
        .Where(m => string.IsNullOrEmpty(searchFilter) || m.Message.Contains(searchFilter, StringComparison.OrdinalIgnoreCase));

    private bool IsInputValidJson => IsValidJson(input);

    private readonly Queue<(DateTime Timestamp, int MessageCount)> throughputSnapshots = new();
    private (DateTime Timestamp, int MessageCount)? firstThroughputSnapshot;
    private double throughput1m;
    private double throughput5m;
    private double throughput15m;
    private double throughputAll;
    private Timer? throughputTimer;

    private bool showChart;

    private string[] GetPartitionChartLabels()
    {
        var partitionIds = eventHub?.PartitionIds ?? [];
        var total = (double)messages.Count;
        return partitionIds
            .Select(id =>
            {
                var count = messages.Count(m => m.PartitionId == id);
                var pct = total > 0 ? count / total * 100 : 0;
                return $"Partition {id}: {count} messages ({pct:F1}%)";
            })
            .ToArray();
    }

    private double[] GetPartitionChartData() => (eventHub?.PartitionIds ?? [])
        .Select(id => (double)messages.Count(m => m.PartitionId == id))
        .ToArray();

    private string CurrentIcon => isPlaying ? Icons.Material.Filled.Pause : Icons.Material.Filled.PlayArrow;

    public EventHub(HttpClient httpClient)
    {
        this.httpClient = httpClient;
        connection = new HubConnectionBuilder().WithUrl(httpClient.BaseAddress + "notifications").Build();
        hub = connection.CreateHubProxy<ILiveExplorerHub>(CancellationToken.None);
        subscription = connection.Register<ILiveExplorerClient>(this);
    }

    protected override async Task OnInitializedAsync()
    {
        await connection.StartAsync(CancellationToken.None);
        throughputTimer = new Timer(
            OnThroughputTick,
            null,
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(1));
    }

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
            throughputSnapshots.Clear();
            firstThroughputSnapshot = null;
        }

        isPlaying = true;

        eventHub = await httpClient.GetEventHubAsync(ServiceKey, cancellationToken);
        lastServiceKey = ServiceKey;
        lastPartitionIds = eventHub!.PartitionIds;

        if (partitionFilter is not null && !eventHub.PartitionIds.Contains(partitionFilter, StringComparer.Ordinal))
        {
            partitionFilter = null;
        }

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
        throughputSnapshots.Clear();
        firstThroughputSnapshot = null;
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

    private static readonly JsonSerializerOptions ExportJsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private static bool IsValidJson(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        try
        {
            using var doc = JsonDocument.Parse(text);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private void FormatInput()
    {
        try
        {
            using var doc = JsonDocument.Parse(input);
            input = JsonSerializer.Serialize(doc, ExportJsonOptions);
        }
        catch (JsonException)
        {
            // Input is not valid JSON, keep as-is
        }
    }

    private void OnThroughputTick(object? state)
    {
        var now = DateTime.UtcNow;
        var currentCount = messages.Count;
        throughputSnapshots.Enqueue((now, currentCount));
        firstThroughputSnapshot ??= (now, currentCount);

        // Trim snapshots older than 15 minutes
        var cutoff = now.AddMinutes(-15).AddSeconds(-1);
        while (throughputSnapshots.Count > 0 && throughputSnapshots.Peek().Timestamp < cutoff)
        {
            throughputSnapshots.Dequeue();
        }

        throughput1m = CalculateThroughputRate(TimeSpan.FromMinutes(1));
        throughput5m = CalculateThroughputRate(TimeSpan.FromMinutes(5));
        throughput15m = CalculateThroughputRate(TimeSpan.FromMinutes(15));

        if (firstThroughputSnapshot is { } first)
        {
            var elapsed = (now - first.Timestamp).TotalSeconds;
            throughputAll = elapsed >= 1 ? Math.Max(0, (currentCount - first.MessageCount) / elapsed) : 0;
        }

        _ = InvokeAsync(StateHasChanged);
    }

    private double CalculateThroughputRate(TimeSpan window)
    {
        if (throughputSnapshots.Count == 0)
        {
            return 0;
        }

        var now = throughputSnapshots.Last().Timestamp;
        var currentCount = throughputSnapshots.Last().MessageCount;
        var windowStart = now - window;

        // Find the snapshot closest to the window start (at or before it)
        (DateTime Timestamp, int MessageCount)? reference = null;
        foreach (var snapshot in throughputSnapshots)
        {
            if (snapshot.Timestamp <= windowStart)
            {
                reference = snapshot;
            }
            else
            {
                break;
            }
        }

        // If no snapshot at/before window start, use the oldest available
        reference ??= throughputSnapshots.Peek();

        var elapsed = (now - reference.Value.Timestamp).TotalSeconds;
        return elapsed >= 1 ? Math.Max(0, (currentCount - reference.Value.MessageCount) / elapsed) : 0;
    }

    private async Task ExportJsonAsync()
    {
        var export = FilteredMessages.Select(m => new
        {
            m.PartitionId,
            m.SequenceNumber,
            EnqueuedTime = m.EnqueuedTime.ToString("o"),
            m.Message,
        });
        var json = JsonSerializer.Serialize(export, ExportJsonOptions);
        await JS.InvokeVoidAsync("fileInterop.download", $"{ServiceKey}-messages.json", "application/json", json);
    }

    private async Task ExportCsvAsync()
    {
        var sb = new StringBuilder();
        sb.AppendLine("PartitionId,SequenceNumber,EnqueuedTime,Message");
        foreach (var m in FilteredMessages)
        {
            sb.Append(m.PartitionId).Append(',');
            sb.Append(m.SequenceNumber).Append(',');
            sb.Append(m.EnqueuedTime.ToString("o")).Append(',');
            sb.Append('"').Append(m.Message.Replace("\"", "\"\"", StringComparison.Ordinal)).AppendLine("\"");
        }

        await JS.InvokeVoidAsync("fileInterop.download", $"{ServiceKey}-messages.csv", "text/csv", sb.ToString());
    }

    private async Task CopyMessageAsync(string message)
    {
        await JS.InvokeVoidAsync("clipboardInterop.writeText", message);
        Snackbar.Add("Copied to clipboard", Severity.Success);
    }

    private Task<IDialogReference> ShowMessageDetailAsync(EventHubMessage message)
    {
        var parameters = new DialogParameters<MessageDetailDialog>
        {
            { x => x.EventHubMessage, message },
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true,
            CloseOnEscapeKey = true,
        };

        return DialogService.ShowAsync<MessageDetailDialog>("Message Detail", parameters, options);
    }

    public async ValueTask DisposeAsync()
    {
        if (throughputTimer is not null)
        {
            await throughputTimer.DisposeAsync();
        }

        subscription?.Dispose();
        processParametersCts?.Dispose();
        await connection.DisposeAsync();
    }
}