var builder = DistributedApplication.CreateBuilder(args);

// Add Event Hub Namespace
var eventHubNamespace = builder.AddAzureEventHubs("event-hub-namespace").RunAsEmulator();

// Add Event Hubs
var eventHub1 = eventHubNamespace.AddHub("event-hub-1");
var eventHub2 = eventHubNamespace.AddHub("event-hub-2");
var eventHub3 = eventHubNamespace.AddHub("event-hub-3");

// Add a custom consumer group for "event-hub-2", for the explorer to use instead of $Default.
// We override the connection name later when referencing this, to avoid it being labeled as "custom" in the explorer.
var eventHub2CustomConsumerGroup = eventHub2.AddConsumerGroup("custom");

// Add Event Hub Live Explorer and reference the Event Hubs
builder
    .AddAzureEventHubsLiveExplorer("event-hub-live-explorer")
    .WithReference(eventHub1)
    .WithReference(eventHub2CustomConsumerGroup, connectionName: eventHub2.Resource.Name)
    .WithReference(eventHub3);

await builder
    .Build()
    .RunAsync();