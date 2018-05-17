using System;

using Collector.Serilog.Enrichers.SensitiveInformation.Util;

using Destructurama.Attributed;

using Serilog.Core;
using Serilog.Events;

namespace Collector.Serilog.Enrichers.SensitiveInformation.Attributed
{
    [AttributeUsage(AttributeTargets.Property)]
    public class LogAsSensitiveAttribute : Attribute, IPropertyDestructuringAttribute
    {
        private readonly SensitiveInformationType? _sensitiveInformationType;

        public LogAsSensitiveAttribute(SensitiveInformationType sensitiveInformationType)
        {
            _sensitiveInformationType = sensitiveInformationType;
        }

        public LogAsSensitiveAttribute()
        {
        }

        public bool TryCreateLogEventProperty(string name, object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventProperty property)
        {
            property = new LogEventProperty(
                name: _sensitiveInformationType?.ToString() ?? name,
                value: propertyValueFactory.CreatePropertyValue(value).AsSensitive());
            return true;
        }
    }
}
