using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Producer;
using LupusBytes.Azure.EventHubs.LiveExplorer.Contracts;
using Microsoft.AspNetCore.SignalR;

namespace LupusBytes.Azure.EventHubs.LiveExplorer.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventHubServices(this IServiceCollection services, IConfiguration configuration)
    {
        foreach (var eventHub in configuration.GetEventHubConnections())
        {
            services.AddSingleton(eventHub);

            services.AddKeyedSingleton(
                eventHub.ServiceKey,
                (sp, serviceKey) => new EventHubService(
                    (string)serviceKey!,
                    new EventHubConsumerClient(eventHub.ConsumerGroup, eventHub.ConnectionString),
                    new EventHubProducerClient(eventHub.ConnectionString),
                    sp.GetRequiredService<IHubContext<LiveExplorerHub, ILiveExplorerHub>>(),
                    sp.GetRequiredService<ILogger<EventHubService>>()));

            services.AddSingleton<IHostedService>(
                sp => sp.GetRequiredKeyedService<EventHubService>(eventHub.ServiceKey));
        }

        services.AddSingleton<EventHubServiceProvider>();

        return services;
    }
}