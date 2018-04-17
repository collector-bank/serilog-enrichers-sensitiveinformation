using System.Collections.Generic;

using Serilog.Events;

namespace Collector.Serilog.SensitiveInformation.Util
{
    internal static class LogEventPropertyValueExtensions
    {
        public static LogEventPropertyValue AsSensitive(this LogEventPropertyValue eventPropertyValue)
        {
            return new DictionaryValue(new Dictionary<ScalarValue, LogEventPropertyValue>
                                       {
                                           [new ScalarValue(SensitiveInformationEnricher.SensitiveInformation)] = eventPropertyValue,
                                       });
        }
    }
}