namespace Collector.Serilog.Enrichers.SensitiveInformation.Examples.Types
{
    public class Email
    {
        public string Value { get; }

        public Email(string value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}