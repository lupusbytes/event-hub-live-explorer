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

        app.MapGet("/api/event-hubs/{serviceKey}", (
            [FromServices] GetEventHubHandler handler,
            string serviceKey)
            => handler.Execute(serviceKey));

        app.MapGet("/api/event-hubs/{serviceKey}/partitions/{partitionId}/events", (
            [FromServices] GetEventHubPartitionEventsHandler handler,
            string serviceKey,
            string partitionId,
            [FromQuery] string? continuationToken)
            => handler.Execute(serviceKey, partitionId, continuationToken));

        return app;
    }
}