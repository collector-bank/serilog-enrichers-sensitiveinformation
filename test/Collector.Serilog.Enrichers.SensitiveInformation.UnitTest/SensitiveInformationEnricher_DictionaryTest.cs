using System.Collections.Generic;

using Xunit;

namespace Collector.Serilog.Enrichers.SensitiveInformation.UnitTest
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

        [Fact]
        public void When_a_dictionary_key_is_blacklisted_then_it_is_marked_as_sensitive()
        {
            Logger.ForContext("Dic", new Dictionary<string, object> { ["Blacklisted"] = "Value" }, destructureObjects: true)
                  .Information("Test");

            Assert.Equal(@"{""__sensitiveInfo"":{""Dic"":{""Blacklisted"":""Value""}}}", Sink.Properties);
        }

        [Fact]
        public void When_a_dictionary_have_some_keys_blacklisted_then_only_those_keys_are_marked_as_sensitive()
        {
            Logger.ForContext("Dic", new Dictionary<string, object>
                                     {
                                         ["Blacklisted"] = "Value",
                                         ["Regular"] = "RegularValue"
                                         
                                     }, destructureObjects: true)
                  .Information("Test");

            Assert.Equal(@"{""Dic"":{""Regular"":""RegularValue""},""__sensitiveInfo"":{""Dic"":{""Blacklisted"":""Value""}}}", Sink.Properties);
        }
    }
}