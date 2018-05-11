using Serilog;
using Serilog.Context;
using Serilog.Enrichers;

namespace Collector.Serilog.Enrichers.SensitiveInformation.Examples.examples
{
    public class BlacklistExample
    {
        public static void Run()
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.With<MachineNameEnricher>()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("SensitiveEnricherProperty", "sensitive enricher property value")
                .Enrich.With(new SensitiveInformationEnricher("B1", "B2", "SensitiveEnricherProperty"))
                .WriteTo.Sink(SerilogConfiguration.CreateAzureEventHubBatchingSink())
                .CreateLogger();

            var logger = Log.Logger.MarkAsReviewedRegardingSensitiveInformation();


            using (LogContext.PushProperty("B1", "blacklisted"))
                logger.ForContext("B2", "blacklisted")
                      .Information("BlacklistExample");

            Log.CloseAndFlush();
        }
    }
}