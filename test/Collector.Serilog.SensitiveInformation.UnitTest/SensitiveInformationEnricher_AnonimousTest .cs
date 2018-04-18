using Xunit;

namespace Collector.Serilog.SensitiveInformation.UnitTest
{
    public partial class SensitiveInformationEnricher_Test
    {
        [Fact]
        public void When_an_anonymous_object_only_have_regular_information()
        {
            Logger.ForContext("Anon", new { Regular = "RegularValue" }, destructureObjects: true)
                  .Information("Test");

            Assert.Equal(@"{""Anon"":{""Regular"":""RegularValue""}}", Sink.Properties);
        }

        [Fact]
        public void When_an_anonymous_object_only_have_sensitive_information()
        {
            Logger.ForContext("Anon", new { Sensitive = new TestClass() }, destructureObjects: true)
                  .Information("Test");

            Assert.Equal(@"{""__sensitiveInfo"":{""Anon"":{""Sensitive"":""SensitiveValue""}}}", Sink.Properties);
        }

        [Fact]
        public void When_an_anonymous_object_have_both_sensitive_and_regular_information()
        {
            Logger.ForContext("Anon", new { Sensitive = new TestClass(), Regular = "RegularValue" }, destructureObjects: true)
                  .Information("Test");

            Assert.Equal(@"{""Anon"":{""Regular"":""RegularValue""},""__sensitiveInfo"":{""Anon"":{""Sensitive"":""SensitiveValue""}}}", Sink.Properties);
        }

        [Fact]
        public void When_a_piece_of_information_is_marked_as_sensitive_but_have_both_sensitive_and_regular_information_then_all_is_marked_as_sensitive()
        {
            Logger.WithSensitiveInformation("Anon", new { Sensitive = new TestClass(), Regular = "RegularValue" })
                  .Information("Test");

            Assert.Equal(@"{""__sensitiveInfo"":{""Anon"":{""Sensitive"":""SensitiveValue"",""Regular"":""RegularValue""}}}", Sink.Properties);
        }

        [Fact]
        public void When_an_anonymous_object_have_a_blacklisted_property_name_then_is_is_marked_as_sensitive()
        {
            Logger.ForContext("SomeName", new { Blacklisted = "Value" }, destructureObjects: true)
                  .Information("Test");

            Assert.Equal(@"{""__sensitiveInfo"":{""SomeName"":{""Blacklisted"":""Value""}}}", Sink.Properties);
        }

        [Fact]
        public void When_an_anonymous_object_have_a_blacklisted_property_name_then_only_those_properties_are_marked_as_sensitive()
        {
            Logger.ForContext("SomeName", new { Blacklisted = "Value", Regular = "RegularValue" }, destructureObjects: true)
                  .Information("Test");

            Assert.Equal(@"{""SomeName"":{""Regular"":""RegularValue""},""__sensitiveInfo"":{""SomeName"":{""Blacklisted"":""Value""}}}", Sink.Properties);
        }
    }
}