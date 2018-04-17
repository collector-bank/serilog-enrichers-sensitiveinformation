using System;

using Collector.Serilog.SensitiveInformation.UnitTest.Helpers;

using Serilog;
using Serilog.Core;

using Xunit;

namespace Collector.Serilog.SensitiveInformation.UnitTest.DestructuringPolicies
{
    public class SensitiveInformationTransformativeDestructuringPolicy_Test
    {
        private Logger Logger { get; }
        private JsonSink Sink { get; }

        public SensitiveInformationTransformativeDestructuringPolicy_Test()
        {
            Sink = new JsonSink();
            Logger = new LoggerConfiguration()
                .Destructure.AsSensitiveByTransforming<TestClass>(tc => tc.Prop1 + tc.Prop2)
                .Enrich.With<SensitiveInformationEnricher>()
                .WriteTo.Sink(Sink)
                .CreateLogger();
        }

        [Fact]
        public void When_destructuring_a_class_as_sensitive_with_a_transform()
        {
            Logger.ForContext("ContextName", new TestClass(), destructureObjects: true)
                  .Information("Test");

            Assert.Equal(@"{""__sensitiveInfo"":{""ContextName"":""Value1Value2""}}", Sink.Properties);
        }

        private class TestClass
        {
            public string Prop1 { get; set; } = "Value1";

            public string Prop2 { get; set; } = "Value2";
        }
    }
}