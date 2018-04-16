using Collector.Serilog.SensitiveInformation.Util;

using Serilog.Core;
using Serilog.Events;

namespace Collector.Serilog.SensitiveInformation.DestructuringPolicies
{
    public class SensitiveInformationDestructuringPolicy<T> : IDestructuringPolicy
    {
        private readonly bool _stringify;
        private object _currentValue;
        
        public SensitiveInformationDestructuringPolicy(bool stringify = false)
        {
            _stringify = stringify;
        }

        public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventPropertyValue result)
        {
            if (value is T tValue && _currentValue != value)
            {
                _currentValue = value;
                var propValue = _stringify ? tValue.ToString() as object : tValue;

                result = propertyValueFactory.CreatePropertyValue(value: propValue, destructureObjects: true).AsSensitive();
                return true;
            }

            result = null;
            return false;
        }
    }
}