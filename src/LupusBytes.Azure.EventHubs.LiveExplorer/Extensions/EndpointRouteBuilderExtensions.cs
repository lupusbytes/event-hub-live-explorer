using LupusBytes.Azure.EventHubs.LiveExplorer.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace LupusBytes.Azure.EventHubs.LiveExplorer.Extensions;

internal static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/event-hubs/", (
            [FromServices] GetEventHubsHandler handler)
            => handler.Execute());

        app.MapGet("/api/event-hubs/{eventHubNamespace}/{name}", (
            [FromServices] GetEventHubHandler handler,
            string eventHubNamespace,
            string name)
            => handler.Execute(eventHubNamespace, name));

        app.MapGet("/api/event-hubs/{eventHubNamespace}/{name}/partitions/{partitionId}/events", (
            [FromServices] GetEventHubPartitionEventsHandler handler,
            string eventHubNamespace,
            string name,
            string partitionId,
            [FromQuery] long? fromSequenceNumber,
            [FromQuery] string? continuationToken)
            => handler.Execute(eventHubNamespace, name, partitionId, fromSequenceNumber, continuationToken));

        return app;
    }
}