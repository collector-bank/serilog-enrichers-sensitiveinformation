using Collector.Serilog.Sinks.AzureEventHub;

using Microsoft.Azure.EventHubs;

using Serilog.Core;

namespace Collector.Serilog.Enrichers.SensitiveInformation.Examples
{
    public static class SerilogConfiguration
    {
        public static ILogEventSink CreateAzureEventHubBatchingSink()
        {
            var client = EventHubClient.CreateFromConnectionString("Add your connectionstring here!");
            return new AzureEventHubSink(eventHubClient: client);
        }
    }
}