namespace Collector.Serilog.Enrichers.SensitiveInformation
{
    public enum SensitiveInformationType
    {
        FirstName,
        LastName,
        FullName,
        IpAddress,
        Address,
        PostalCode,
        City,
        RegistrationNumber,
        Email,
        PhoneNumber,
        RequestBody,
        ResponseBody,
    }
}