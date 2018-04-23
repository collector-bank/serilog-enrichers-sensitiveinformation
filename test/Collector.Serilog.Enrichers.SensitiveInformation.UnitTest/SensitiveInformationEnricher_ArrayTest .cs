using Xunit;

namespace Collector.Serilog.Enrichers.SensitiveInformation.UnitTest
{
    public partial class SensitiveInformationEnricher_Test
    {
        [Fact]
        public void When_a_list_only_have_regular_information()
        {
            Logger.ForContext("List", new object[] { "RegularValue" }, destructureObjects: true)
                  .Information("Test");

            Assert.Equal(@"{""List"":[""RegularValue""]}", Sink.Properties);
        }

        [Fact]
        public void When_a_list_only_have_sensitive_information()
        {
            Logger.ForContext("List", new object[] { new TestClass() }, destructureObjects: true)
                  .Information("Test");

            Assert.Equal(@"{""__sensitiveInfo"":{""List"":[""SensitiveValue""]}}", Sink.Properties);
        }

        [Fact]
        public void When_a_list_have_both_sensitive_and_regular_information()
        {
            Logger.ForContext("List", new object[] { new TestClass(), "RegularValue" }, destructureObjects: true)
                  .Information("Test");

            Assert.Equal(@"{""List"":[""RegularValue""],""__sensitiveInfo"":{""List"":[""SensitiveValue""]}}", Sink.Properties);
        }
    }
}