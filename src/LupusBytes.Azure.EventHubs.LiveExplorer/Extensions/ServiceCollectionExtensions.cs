using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Producer;
using LupusBytes.Azure.EventHubs.LiveExplorer.Contracts;
using LupusBytes.Azure.EventHubs.LiveExplorer.Contracts.SignalR;
using LupusBytes.Azure.EventHubs.LiveExplorer.Handlers;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
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
                    eventHub.Endpoint,
                    new EventHubConsumerClient(eventHub.ConsumerGroup, eventHub.ConnectionString),
                    new EventHubProducerClient(eventHub.ConnectionString),
                    sp.GetRequiredService<IHubContext<LiveExplorerHub, ILiveExplorerClient>>(),
                    sp.GetRequiredService<ILogger<EventHubService>>()));

            services.AddSingleton<IHostedService>(
                sp => sp.GetRequiredKeyedService<EventHubService>(eventHub.ServiceKey));
        }

        services.AddSingleton<EventHubServiceProvider>();

        return services;
    }

    public static IServiceCollection AddPrerenderServices(this IServiceCollection services)
    {
        services.AddScoped<HttpClient>(sp =>
        {
            var serverAddress = sp
                .GetRequiredService<IServer>().Features
                .Get<IServerAddressesFeature>()?.Addresses
                .FirstOrDefault() ?? throw new InvalidOperationException("Could not find any server addresses");

            var baseAddress = new Uri(serverAddress);

            // Replace 0.0.0.0 and [::] with localhost
            if (baseAddress.Host is "0.0.0.0" or "[::]")
            {
                baseAddress = new UriBuilder(baseAddress) { Host = "localhost" }.Uri;
            }

            return new HttpClient { BaseAddress = baseAddress };
        });

        return services;
    }

    public static IServiceCollection AddApiEndpointHandlers(this IServiceCollection services)
        => services
            .AddSingleton<GetEventHubsHandler>()
            .AddSingleton<GetEventHubHandler>()
            .AddSingleton<GetEventHubPartitionEventsHandler>();
}