using System.Diagnostics.CodeAnalysis;

namespace LupusBytes.Azure.EventHubs.LiveExplorer.Web;

internal class EventHubServiceProvider(IServiceProvider serviceProvider)
{
    private readonly Dictionary<string, EventHubService> eventHubServices = serviceProvider
        .GetServices<EventHubConnectionInfo>()
        .ToDictionary(
            x => x.ServiceKey,
            x => serviceProvider.GetRequiredKeyedService<EventHubService>(x.ServiceKey),
            StringComparer.Ordinal);

    public EventHubService GetEventHubService(string serviceKey)
        => TryGetEventHubService(serviceKey, out var eventHubService)
            ? eventHubService
            : throw new ArgumentException(
                $"No EventHubService registered with serviceKey '{serviceKey}'",
                nameof(serviceKey));

    public bool TryGetEventHubService(
        string serviceKey,
        [NotNullWhen(true)] out EventHubService? eventHubService)
        => eventHubServices.TryGetValue(serviceKey, out eventHubService);
}