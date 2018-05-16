using System;

using Collector.Serilog.Enrichers.SensitiveInformation.Util;

using Serilog.Core;
using Serilog.Events;

namespace Collector.Serilog.Enrichers.SensitiveInformation.DestructuringPolicies
{
    internal class SensitiveInformationDestructuringPolicy<T> : IDestructuringPolicy
    {
        [ThreadStatic]
        // ReSharper disable once StaticMemberInGenericType, this is by intent.
        private static object currentValue;

        private readonly bool _stringify;
        
        public SensitiveInformationDestructuringPolicy(bool stringify = false)
        {
            _stringify = stringify;
        }

        public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventPropertyValue result)
        {
            if (value is T tValue && currentValue != value)
            {
                currentValue = value;
                var propValue = _stringify ? tValue.ToString() as object : tValue;

                result = propertyValueFactory.CreatePropertyValue(value: propValue, destructureObjects: true).AsSensitive();
                currentValue = null;
                return true;
            }

            result = null;
            return false;
        }
    }
}