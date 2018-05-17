using System;
using System.Linq.Expressions;

using Collector.Serilog.Enrichers.SensitiveInformation.UnitTest.Helpers;

using Destructurama;
using Destructurama.Attributed;

using Serilog;
using Serilog.Core;

using Xunit;

namespace Collector.Serilog.Enrichers.SensitiveInformation.Attributed.UnitTest
{
    public class LogAsSensitiveAttribute_Test
    {
        private JsonSink Sink { get; }

        public LogAsSensitiveAttribute_Test()
        {
            Sink = new JsonSink();
        }

        private Logger CreateLogger()
        {
            return new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .Enrich.With(new SensitiveInformationEnricher())
                .WriteTo.Sink(Sink)
                .CreateLogger();
        }

        [Fact]
        public void When_destructuring_a_class_with_attributes()
        {
            var logger = CreateLogger();

            logger.ForContext("ContextName", new TestClass(), destructureObjects: true)
                  .Information("Test");

            Assert.Equal(@"{""ContextName"":{""_typeTag"":""TestClass"",""Prop1"":""Value1""},""__sensitiveInfo"":{""ContextName"":{""_typeTag"":""TestClass"",""Prop2"":""Value2""}}}", Sink.Properties);
        }

        [Fact]
        public void When_destructuring_a_class_with_attributes_and_a_given_name()
        {
            var logger = CreateLogger();

            logger.ForContext("ContextName", new TestClass2(), destructureObjects: true)
                  .Information("Test");

            Assert.Equal(@"{""ContextName"":{""_typeTag"":""TestClass2"",""Prop1"":""Value1""},""__sensitiveInfo"":{""ContextName"":{""_typeTag"":""TestClass2"",""Email"":""Value2""}}}", Sink.Properties);
        }


        private class TestClass
        {
            public string Prop1 { get; set; } = "Value1";

            [LogAsSensitive]
            public string Prop2 { get; set; } = "Value2";

            [NotLogged]
            public string Prop3 { get; set; } = "Value3";
        }


        private class TestClass2
        {
            public string Prop1 { get; set; } = "Value1";

            [LogAsSensitive(SensitiveInformationType.Email)]
            public string Prop2 { get; set; } = "Value2";

            [NotLogged]
            public string Prop3 { get; set; } = "Value3";
        }
    }
}