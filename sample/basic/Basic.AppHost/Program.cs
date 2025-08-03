var builder = DistributedApplication.CreateBuilder(args);

// Add Event Hub Namespace
var eventHubNamespace = builder.AddAzureEventHubs("event-hub-namespace").RunAsEmulator();

// Add Event Hubs
var eventHub1 = eventHubNamespace.AddHub("event-hub-1");
var eventHub2 = eventHubNamespace.AddHub("event-hub-2");

// Use a custom consumer group for "event-hub-2" instead of $Default
var eventHub2WithCustomConsumerGroup = eventHub2.AddConsumerGroup("custom");

builder
    .AddAzureEventHubsLiveExplorer("event-hub-live-explorer")
    .WithReference(eventHub1)
    .WithReference(eventHub2WithCustomConsumerGroup, connectionName: eventHub2.Resource.Name);

await builder
    .Build()
    .RunAsync();