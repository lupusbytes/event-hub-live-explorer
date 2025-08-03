var builder = Host.CreateApplicationBuilder(args);

builder.AddKeyedAzureEventHubProducerClient("humidity");
builder.AddKeyedAzureEventHubProducerClient("temperature");
builder.AddKeyedAzureEventHubProducerClient("system-events");
builder.AddKeyedAzureEventHubProducerClient("user-events");

builder.Services.AddHostedService<HumidityEventHubProducer>();
builder.Services.AddHostedService<TemperatureEventHubProducer>();
builder.Services.AddHostedService<SystemEventHubProducer>();
builder.Services.AddHostedService<UserEventHubProducer>();

await builder
    .Build()
    .RunAsync();