namespace Collector.Serilog.Enrichers.SensitiveInformation.Examples.Types
{
    public class BankAccount
    {
        public string AccountNumber { get; }
        public BankAccountType BankAccountType { get; }

        public BankAccount(string accountNumber, BankAccountType bankAccountType)
        {
            AccountNumber = accountNumber;
            BankAccountType = bankAccountType;
        }
    }
}