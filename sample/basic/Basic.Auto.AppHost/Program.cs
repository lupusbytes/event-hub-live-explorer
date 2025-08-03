var builder = DistributedApplication.CreateBuilder(args);

var eventHubNamespace = builder.AddAzureEventHubs("event-hub-namespace").RunAsEmulator();
eventHubNamespace.AddHub("event-hub-1");
eventHubNamespace.AddHub("event-hub-2");
eventHubNamespace.AddHub("event-hub-3");
eventHubNamespace.AddHub("event-hub-4");

builder
    .AddAzureEventHubsLiveExplorer("event-hub-live-explorer")
    .WithAutoReferences(consumerGroupName: "explorer"); // Remove optional consumer group name to use $Default

await builder
    .Build()
    .RunAsync();