using System.Collections.Generic;

using Xunit;

namespace Collector.Serilog.SensitiveInformation.UnitTest
{
    public partial class SensitiveInformationEnricher_Test
    {
        [Fact]
        public void When_a_dictionary_only_have_regular_information()
        {
            Logger.ForContext("Dic", new Dictionary<string, object> { ["Regular"] = "RegularValue" }, destructureObjects: true)
                  .Information("Test");

            Assert.Equal(@"{""Dic"":{""Regular"":""RegularValue""}}", Sink.Properties);
        }

        [Fact]
        public void When_a_dictionary_only_have_sensitive_information()
        {
            Logger.ForContext("Dic", new Dictionary<string, object> { ["Sensitive"] = new TestClass() }, destructureObjects: true)
                  .Information("Test");

            Assert.Equal(@"{""__sensitiveInfo"":{""Dic"":{""Sensitive"":""SensitiveValue""}}}", Sink.Properties);
        }

        [Fact]
        public void When_a_dictionary_have_both_sensitive_and_regular_information()
        {
            Logger.ForContext("Dic", new Dictionary<string, object> { ["Sensitive"] = new TestClass(), ["Regular"] = "RegularValue" }, destructureObjects: true)
                  .Information("Test");

            Assert.Equal(@"{""Dic"":{""Regular"":""RegularValue""},""__sensitiveInfo"":{""Dic"":{""Sensitive"":""SensitiveValue""}}}", Sink.Properties);
        }
    }
}