// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SensitiveLoggingExtentions_Test.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Collector.Serilog.SensitiveInformation.UnitTest.Helpers;

using Serilog;
using Serilog.Core;

using Xunit;

namespace Collector.Serilog.SensitiveInformation.UnitTest
{
    public class SensitiveLoggingExtentions_Test
    {
        public Logger Logger { get; }

        private JsonSink Sink { get; }

        public SensitiveLoggingExtentions_Test()
        {
            Sink = new JsonSink();
            Logger = new LoggerConfiguration()
                .WriteTo.Sink(Sink)
                .CreateLogger();
        }

        [Fact]
        public void When_marking_a_log_as_reviewed_for_sensetive_information_then_it_ads_a_property_to_the_log_event()
        {
            Logger.MarkAsReviewedRegardingSensitiveInformation()
                  .Information("Test");

            Assert.Equal(@"{""__sensitiveInfoHasBeenReviewed"":true}", Sink.Properties);
        }
    }
}