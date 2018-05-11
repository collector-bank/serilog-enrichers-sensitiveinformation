using Serilog;
using Serilog.Context;
using Serilog.Enrichers;

namespace Collector.Serilog.Enrichers.SensitiveInformation.Examples.examples
{
    public class WithSensitiveInformationExample
    {
        public static void Run()
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.With<MachineNameEnricher>()
                .Enrich.FromLogContext()
                .Enrich.With(new SensitiveInformationEnricher())
                .WriteTo.Sink(SerilogConfiguration.CreateAzureEventHubBatchingSink())
                .CreateLogger();

            var logger = Log.Logger.MarkAsReviewedRegardingSensitiveInformation();


            logger.ForContext("RegularProp", "regular value")
                  .WithSensitiveInformation("SensitiveProp", "sensitive value")
                  .Information("WithSensitiveInformation");

            using (SensitiveLogContext.PushProperty("SensitiveProp", "sensitive value"))
            using (LogContext.PushProperty("RegularProp", "regular value"))
                logger.Information("SensitiveLogContext");

            Log.CloseAndFlush();
        }
    }
}