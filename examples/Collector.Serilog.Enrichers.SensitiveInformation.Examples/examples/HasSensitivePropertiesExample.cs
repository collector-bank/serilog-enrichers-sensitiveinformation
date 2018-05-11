using Collector.Serilog.Enrichers.SensitiveInformation.Examples.Types;

using Serilog;
using Serilog.Enrichers;

namespace Collector.Serilog.Enrichers.SensitiveInformation.Examples.examples
{
    public class HasSensitivePropertiesExample
    {
        public static void Run()
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.With<MachineNameEnricher>()
                .Enrich.With(new SensitiveInformationEnricher())
                .Destructure.HasSensitiveProperties<BankAccount>(b => b.AccountNumber)
                .WriteTo.Sink(SerilogConfiguration.CreateAzureEventHubBatchingSink())
                .CreateLogger();

            var logger = Log.Logger.MarkAsReviewedRegardingSensitiveInformation();

            var engagement = new
                             {
                                 Merchant = "Merchant name",
                                 BankAccount = new BankAccount("number", BankAccountType.SwedishBankAccount)
                             };

            logger.ForContext("Engagement", engagement, destructureObjects: true)
                  .Information("HasSensitivePropertiesExample");

            Log.CloseAndFlush();
        }
    }
}