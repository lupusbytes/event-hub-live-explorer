using LupusBytes.Azure.EventHubs.LiveExplorer.Web;
using LupusBytes.Azure.EventHubs.LiveExplorer.Web.Components;
using LupusBytes.Azure.EventHubs.LiveExplorer.Web.Contracts;
using LupusBytes.Azure.EventHubs.LiveExplorer.Web.Extensions;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMudServices()
    .AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services
    .AddEventHubServices(builder.Configuration)
    .AddSignalR();

var app = builder.Build();

app.MapHub<LiveExplorerHub>("notifications");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(LupusBytes.Azure.EventHubs.LiveExplorer.Web.Client._Imports).Assembly);

app.MapGet("/api/event-hubs/{serviceKey}/messages", (
    string serviceKey,
    EventHubServiceProvider serviceProvider)
    => serviceProvider.TryGetEventHubService(serviceKey, out var eventHubService)
        ? Results.Json(eventHubService.Messages)
        : Results.NotFound());

app.MapGet("/api/event-hubs/", (
    IEnumerable<EventHubConnectionInfo> eventHubConnections)
    => Results.Json(eventHubConnections.Select(x => new EventHubInfo(x.Endpoint, x.ServiceKey))));

await app.RunAsync();

