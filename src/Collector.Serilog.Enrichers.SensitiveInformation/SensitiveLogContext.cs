using System;
using System.Collections.Generic;

using Collector.Serilog.Enrichers.SensitiveInformation.Util;

using Serilog.Context;

namespace Collector.Serilog.Enrichers.SensitiveInformation
{
    public static class SensitiveLogContext
    {
        public static IDisposable PushProperty(SensitiveInformationType name, object value)
        {
            return PushProperty(name.ToString(), value);
        }

        public static IDisposable PushProperty(string name, object value)
        {
            return LogContext.PushProperty(name, new Dictionary<string, object> { [Constants.SensitiveInformation] = value }, destructureObjects: true);
        }
    }
}