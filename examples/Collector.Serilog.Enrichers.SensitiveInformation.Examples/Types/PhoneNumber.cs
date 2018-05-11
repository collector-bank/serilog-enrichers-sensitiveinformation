namespace Collector.Serilog.Enrichers.SensitiveInformation.Examples.Types
{
    public class PhoneNumber
    {
        public string CountryPrefix { get; }
        public string Localnumber { get; }

        public PhoneNumber(string countryPrefix, string localnumber)
        {
            CountryPrefix = countryPrefix;
            Localnumber = localnumber;
        }
    }
}