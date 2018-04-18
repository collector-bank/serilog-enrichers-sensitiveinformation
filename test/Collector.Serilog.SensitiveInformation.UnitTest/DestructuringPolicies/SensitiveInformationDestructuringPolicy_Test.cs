using System;

using Collector.Serilog.SensitiveInformation.UnitTest.Helpers;

using Serilog;
using Serilog.Core;

using Xunit;

namespace Collector.Serilog.SensitiveInformation.UnitTest.DestructuringPolicies
{
    public class SensitiveInformationDestructuringPolicy_Test
    {
        private JsonSink Sink { get; }

        public SensitiveInformationDestructuringPolicy_Test()
        {
            Sink = new JsonSink();
        }

        private Logger CreateLogger(bool stringify)
        {
            return new LoggerConfiguration()
                .Destructure.AsSensitive<TestClass>(stringify)
                .Enrich.With(new SensitiveInformationEnricher())
                .WriteTo.Sink(Sink)
                .CreateLogger();
        }

        [Theory]
        [InlineData(true, @"{""__sensitiveInfo"":{""ContextName"":""MyToString""}}")]
        [InlineData(false, @"{""__sensitiveInfo"":{""ContextName"":{""_typeTag"":""TestClass"",""Value"":""name""}}}")]
        public void When_destructuring_a_class_without_transformation(bool stringify, string expectedProperties)
        {
            var logger = CreateLogger(stringify);

            logger.ForContext("ContextName", new TestClass { Value = "name" }, destructureObjects: true)
                  .Information("Test");

            Assert.Equal(expectedProperties, Sink.Properties);
        }

        private class TestClass
        {
            public string Value { get; set; }

            public override string ToString()
            {
                return "MyToString";
            }
        }
    }
}