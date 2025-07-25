using LupusBytes.Azure.EventHubs.LiveExplorer;
using LupusBytes.Azure.EventHubs.LiveExplorer.Components;
using LupusBytes.Azure.EventHubs.LiveExplorer.Contracts;
using LupusBytes.Azure.EventHubs.LiveExplorer.Extensions;
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
    .AddAdditionalAssemblies(typeof(LupusBytes.Azure.EventHubs.LiveExplorer.Client._Imports).Assembly);

app.MapGet("/api/event-hubs/", (
    IEnumerable<EventHubConnectionInfo> eventHubConnections)
    => Results.Json(eventHubConnections.Select(x => new EventHubInfo(x.Endpoint, x.ServiceKey))));

await app.RunAsync();

