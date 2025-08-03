var builder = DistributedApplication.CreateBuilder(args);

// Add Event Hub Live Explorer to Aspire Dashboard
var explorer = builder.AddAzureEventHubsLiveExplorer("event-hub-live-explorer");

// Add Event Hub Namespace with "humidity" and "temperature" Event Hubs
var eventHubNamespace1 = builder.AddAzureEventHubs("event-hub-namespace1").RunAsEmulator();
var humidity = eventHubNamespace1.AddHub("humidity");
var temperature = eventHubNamespace1.AddHub("temperature").WithProperties(x => x.PartitionCount = 4);

// Add another Event Hub Namespace with "user-events" and "system-events" Event Hubs
var eventHubNamespace2 = builder.AddAzureEventHubs("event-hub-namespace2").RunAsEmulator();
var userEvents = eventHubNamespace2.AddHub("user-events");
var systemEvents = eventHubNamespace2.AddHub("system-events");

// Automatically add all Event Hubs to the Event Hub Live Explorer and use "explorer" as the consumer group name.
explorer.WithAutoReferences(consumerGroupName: "explorer");

// Add sample event producer
builder
    .AddProject<EventProducer>("event-producer")
    .WithReference(humidity)
    .WithReference(temperature)
    .WithReference(systemEvents)
    .WithReference(userEvents)
    .WaitFor(humidity)
    .WaitFor(temperature)
    .WaitFor(systemEvents)
    .WaitFor(userEvents);

await builder
    .Build()
    .RunAsync();