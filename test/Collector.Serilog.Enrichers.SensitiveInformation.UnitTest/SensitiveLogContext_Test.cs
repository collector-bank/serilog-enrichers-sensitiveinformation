using System;

using Collector.Serilog.Enrichers.SensitiveInformation.UnitTest.Helpers;

using Serilog;
using Serilog.Context;
using Serilog.Core;

using Xunit;

namespace Collector.Serilog.Enrichers.SensitiveInformation.UnitTest
{
    public class SensitiveLogContext_Test
    {
        public Logger Logger { get; }

        private JsonSink Sink { get; }

        public SensitiveLogContext_Test()
        {
            Sink = new JsonSink();
            Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.With(new SensitiveInformationEnricher("Blacklisted"))
                .WriteTo.Sink(Sink)
                .CreateLogger();
        }

        [Fact]
        public void When_a_context_has_no_sensitive_information_then_nothing_is_modified()
        {
            using (LogContext.PushProperty("Regular", "RegularValue"))
                Logger.Information("Test");

            Assert.Equal(@"{""Regular"":""RegularValue""}", Sink.Properties);
        }

        [Fact]
        public void When_a_context_only_has_sensitive_information_then_only_a_sensitive_property_is_present()
        {
            using (SensitiveLogContext.PushProperty(SensitiveInformationType.FirstName, "SensitiveValue"))
                Logger.Information("Test");

            Assert.Equal(@"{""__sensitiveInfo"":{""FirstName"":""SensitiveValue""}}", Sink.Properties);
        }

        [Fact]
        public void When_a_context_has_both_sensitive_and_regular_information_then_both_types_are_present()
        {
            using (LogContext.PushProperty("Regular", "RegularValue"))
            using (SensitiveLogContext.PushProperty("Sensitive", "SensitiveValue"))
                Logger.Information("Test");

            Assert.Equal(@"{""__sensitiveInfo"":{""Sensitive"":""SensitiveValue""},""Regular"":""RegularValue""}", Sink.Properties);
        }

        [Fact]
        public void When_a_property_name_is_blacklisted_then_is_is_marked_as_sensitive()
        {
            using (LogContext.PushProperty("Blacklisted", "Value"))
                Logger.Information("Test");

            Assert.Equal(@"{""__sensitiveInfo"":{""Blacklisted"":""Value""}}", Sink.Properties);
        }
    }
}