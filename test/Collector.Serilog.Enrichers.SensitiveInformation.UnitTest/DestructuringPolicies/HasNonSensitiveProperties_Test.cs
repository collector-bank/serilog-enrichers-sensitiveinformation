using System;
using System.Linq.Expressions;

using Collector.Serilog.Enrichers.SensitiveInformation.UnitTest.Helpers;

using Serilog;
using Serilog.Core;

using Xunit;

namespace Collector.Serilog.Enrichers.SensitiveInformation.UnitTest.DestructuringPolicies
{
    public class HasNonSensitiveProperties_Test
    {
        private JsonSink Sink { get; }

        public HasNonSensitiveProperties_Test()
        {
            Sink = new JsonSink();
        }

        private Logger CreateLogger(params Expression<Func<TestClass, object>>[] properties)
        {
            return new LoggerConfiguration()
                .Destructure.HasNonSensitiveProperties(properties)
                .Enrich.With(new SensitiveInformationEnricher())
                .WriteTo.Sink(Sink)
                .CreateLogger();
        }

        [Fact]
        public void When_destructuring_a_class_as_sensitive_with_an_expression()
        {
            var logger = CreateLogger(tc => tc.Prop2);

            logger.ForContext("ContextName", new TestClass(), destructureObjects: true)
                  .Information("Test");

            Assert.Equal(@"{""ContextName"":{""Prop2"":""Value2""},""__sensitiveInfo"":{""ContextName"":{""Prop1"":""Value1"",""Prop3"":""Value3""}}}", Sink.Properties);
        }


        private class TestClass
        {
            public string Prop1 { get; set; } = "Value1";

            public string Prop2 { get; set; } = "Value2";

            public string Prop3 { get; set; } = "Value3";
        }
    }
}