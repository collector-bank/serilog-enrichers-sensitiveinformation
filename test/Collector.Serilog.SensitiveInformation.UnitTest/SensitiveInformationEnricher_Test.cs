using System;

using Collector.Serilog.SensitiveInformation.UnitTest.Helpers;

using Serilog;
using Serilog.Core;

using Xunit;

namespace Collector.Serilog.SensitiveInformation.UnitTest
{
    public partial class SensitiveInformationEnricher_Test
    {
        public Logger Logger { get; }

        private JsonSink Sink { get; }

        public SensitiveInformationEnricher_Test()
        {
            Sink = new JsonSink();
            Logger = new LoggerConfiguration()
                .Destructure.AsSensitiveByTransforming<TestClass>(tc => tc.Value)
                .Enrich.With(new SensitiveInformationEnricher("Blacklisted"))
                .WriteTo.Sink(Sink)
                .CreateLogger();
        }

        [Fact]
        public void The_blacklist_must_be_not_null()
        {
            Assert.Throws<ArgumentNullException>(() => new SensitiveInformationEnricher(null));
        }

        [Fact]
        public void Blacklisted_elements_must_be_not_null()
        {
            Assert.Throws<ArgumentException>(() => new SensitiveInformationEnricher(null as string));
        }

        [Fact]
        public void When_a_context_has_no_sensitive_information_then_nothing_is_modified()
        {
            Logger.ForContext("Regular", "RegularValue")
                  .Information("Test");

            Assert.Equal(@"{""Regular"":""RegularValue""}", Sink.Properties);
        }

        [Fact]
        public void When_a_context_only_has_sensitive_information_then_only_a_sensitive_property_is_present()
        {
            Logger.WithSensitiveInformation(SensitiveInformationType.FirstName, "SensitiveValue")
                  .Information("Test");

            Assert.Equal(@"{""__sensitiveInfo"":{""FirstName"":""SensitiveValue""}}", Sink.Properties);
        }

        [Fact]
        public void When_a_context_has_both_sensitive_and_regular_information_then_both_types_are_present()
        {
            Logger.ForContext("Regular", "RegularValue")
                  .WithSensitiveInformation("Sensitive", "SensitiveValue")
                  .Information("Test");

            Assert.Equal(@"{""__sensitiveInfo"":{""Sensitive"":""SensitiveValue""},""Regular"":""RegularValue""}", Sink.Properties);
        }

        [Fact]
        public void When_a_property_name_is_blacklisted_then_is_is_marked_as_sensitive()
        {
            Logger.ForContext("Blacklisted", "Value")
                  .Information("Test");

            Assert.Equal(@"{""__sensitiveInfo"":{""Blacklisted"":""Value""}}", Sink.Properties);
        }

        private class TestClass
        {
            public string Value { get; set; } = "SensitiveValue";
        }
    }
}