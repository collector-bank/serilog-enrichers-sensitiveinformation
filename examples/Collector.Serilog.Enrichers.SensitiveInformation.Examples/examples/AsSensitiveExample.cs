using Collector.Serilog.Enrichers.SensitiveInformation.Examples.Types;

using Serilog;
using Serilog.Enrichers;

namespace Collector.Serilog.Enrichers.SensitiveInformation.Examples.examples
{
    public class AsSensitiveExample
    {
        public static void Run()
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.With<MachineNameEnricher>()
                .Enrich.With(new SensitiveInformationEnricher())
                .Destructure.AsSensitive<Email>()
                .Destructure.AsSensitive<BankAccount>(stringify: false)
                .Destructure.AsSensitiveByTransforming<PhoneNumber>(pn => pn.CountryPrefix + pn.Localnumber)
                .WriteTo.Sink(SerilogConfiguration.CreateAzureEventHubBatchingSink())
                .CreateLogger();

            var logger = Log.Logger.MarkAsReviewedRegardingSensitiveInformation();

            var engagement = new
                             {
                                 Merchant = "Merchant name",
                                 Email = new Email("anon@test.com"),
                                 PhoneNumber = new PhoneNumber("+46", "0123456789"),
                                 BankAccount = new BankAccount("number", BankAccountType.SwedishBankAccount)
                             };

            logger.ForContext("Engagement", engagement, destructureObjects: true)
                  .Information("AsSensitiveExample");

            Log.CloseAndFlush();
        }
    }
}