using System.Diagnostics.CodeAnalysis;

namespace LupusBytes.Azure.EventHubs.LiveExplorer;

internal class EventHubServiceProvider(IServiceProvider serviceProvider)
{
    private readonly Dictionary<string, EventHubService> eventHubServices = serviceProvider
        .GetServices<EventHubConnectionInfo>()
        .ToDictionary(
            x => x.Id,
            x => serviceProvider.GetRequiredKeyedService<EventHubService>(x.Id),
            StringComparer.Ordinal);

    public EventHubService GetEventHubService(string eventHubNamespace, string name)
        => TryGetEventHubService(eventHubNamespace, name, out var eventHubService)
            ? eventHubService
            : throw new ArgumentException(
                $"No EventHubService registered for Event Hub '{name}' in namespace '{eventHubNamespace}'",
                nameof(name));

    public bool TryGetEventHubService(
        string eventHubNamespace,
        string name,
        [NotNullWhen(true)] out EventHubService? eventHubService)
        => eventHubServices.TryGetValue($"{eventHubNamespace}/{name}", out eventHubService);

    public IEnumerable<EventHubService> GetEventHubServices()
        => eventHubServices.Values;
}