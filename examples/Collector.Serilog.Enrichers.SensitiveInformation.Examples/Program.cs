using Collector.Serilog.Enrichers.SensitiveInformation.Examples.examples;

namespace Collector.Serilog.Enrichers.SensitiveInformation.Examples
{
    class Program
    {
        static void Main(string[] args)
        {
            AsSensitiveExample.Run();
            HasSensitivePropertiesExample.Run();
            WithSensitiveInformationExample.Run();
            BlacklistExample.Run();
        }
    }
}
