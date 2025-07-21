using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting;

public static class DistributedApplicationBuilderExtensions
{
    /// <summary>
    /// Adds an Azure Event Hubs Live Explorer resource to the application model.
    /// This resource can be used to interact with Event Hub resources.
    /// </summary>
    /// <param name="builder">The builder for the distributed application.</param>
    /// <param name="name">The name of the resource.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<AzureEventHubsLiveExplorerResource> AddAzureEventHubsLiveExplorer(
        this IDistributedApplicationBuilder builder,
        [ResourceName] string name)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrEmpty(name);

        return builder
            .AddResource(new AzureEventHubsLiveExplorerResource(name))
            .WithImage(ImageConstants.Image)
            .WithImageRegistry(ImageConstants.Registry)
            .WithImageTag(ImageConstants.Tag)
            .WithHttpEndpoint(targetPort: 5000)
            .WithUrlForEndpoint("http", u => u.DisplayText = "Event Hub Live Explorer");
    }
}