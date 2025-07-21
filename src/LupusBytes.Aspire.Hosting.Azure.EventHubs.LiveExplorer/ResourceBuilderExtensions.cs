using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Azure;

namespace Aspire.Hosting;

public static class ResourceBuilderExtensions
{
    /// <summary>
    /// Scans the application model for Azure Event Hubs resources and adds them as references to this resource, so they can be explored.
    /// </summary>
    /// <param name="builder">The Azure Event Hubs Live Explorer resource builder</param>
    /// <param name="consumerGroupName">
    /// The consumer group name which the explorer should use when reading events from the Azure Event Hubs.
    /// If the consumer group does not already exist on a given Azure Event Hub, it will be created automatically.
    /// </param>
    /// <remarks>
    /// This method should only be invoked after every desired Azure Event Hub has already been added.
    /// Azure Event Hubs added after invocation of this method will not be referenced.
    /// </remarks>
    /// <returns>The <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<AzureEventHubsLiveExplorerResource> WithAutoReferences(
        this IResourceBuilder<AzureEventHubsLiveExplorerResource> builder,
        string consumerGroupName = "$Default")
    {
        var hubs = builder.ApplicationBuilder.Resources.OfType<AzureEventHubResource>().ToList();

        foreach (var hub in hubs)
        {
            var hubBuilder = builder
                .ApplicationBuilder
                .CreateResourceBuilder<AzureEventHubResource>(hub.Name);

            IResourceBuilder<IResourceWithConnectionString> reference;
            if (consumerGroupName == "$Default")
            {
                reference = hubBuilder;
            }
            else
            {
                var existingConsumerGroup = builder
                    .ApplicationBuilder
                    .Resources
                    .OfType<AzureEventHubConsumerGroupResource>()
                    .SingleOrDefault(x =>
                        ReferenceEquals(x.Parent, hub) &&
                        x.ConsumerGroupName == consumerGroupName);

                if (existingConsumerGroup is null)
                {
                    reference = hubBuilder.AddConsumerGroup($"{hub.Name}-{consumerGroupName}", consumerGroupName);
                }
                else
                {
                    reference = builder
                        .ApplicationBuilder
                        .CreateResourceBuilder<AzureEventHubConsumerGroupResource>(existingConsumerGroup.Name);
                }
            }

            builder.WithReference(reference, hub.Name);
        }

        return builder;
    }
}