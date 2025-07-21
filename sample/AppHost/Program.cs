var builder = DistributedApplication.CreateBuilder(args);

var eventHubNamespace1 = builder.AddAzureEventHubs("event-hub-namespace1").RunAsEmulator();
eventHubNamespace1.AddHub("temperature");
eventHubNamespace1.AddHub("humidity");

var eventHubNamespace2 = builder.AddAzureEventHubs("event-hub-namespace2").RunAsEmulator();
eventHubNamespace2.AddHub("user-events");
eventHubNamespace2.AddHub("system-events");

builder
    .AddAzureEventHubsLiveExplorer("event-hub-live-explorer")
    .WithAutoReferences(consumerGroupName: "explorer");

await builder
    .Build()
    .RunAsync();