using Collector.Serilog.Enrichers.SensitiveInformation.Examples.examples;

namespace Collector.Serilog.Enrichers.SensitiveInformation.Examples
{
    class Program
    {
        static void Main(string[] args)
        {
            WithSensitiveInformationExample.Run();
            AsSensitiveExample.Run();
            HasSensitivePropertiesExample.Run();
            BlacklistExample.Run();
        }
    }
}
