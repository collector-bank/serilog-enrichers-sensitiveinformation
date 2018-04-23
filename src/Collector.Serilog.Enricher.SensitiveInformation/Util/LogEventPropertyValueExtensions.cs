using System.Collections.Generic;

using Serilog.Events;

namespace Collector.Serilog.Enricher.SensitiveInformation.Util
{
    internal static class LogEventPropertyValueExtensions
    {
        public static LogEventPropertyValue AsSensitive(this LogEventPropertyValue eventPropertyValue)
        {
            return new DictionaryValue(new Dictionary<ScalarValue, LogEventPropertyValue>
                                       {
                                           [new ScalarValue(Constants.SensitiveInformation)] = eventPropertyValue,
                                       });
        }
    }
}