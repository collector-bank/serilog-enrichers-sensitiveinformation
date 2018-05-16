using System;

using Collector.Serilog.Enrichers.SensitiveInformation.Util;

using Serilog.Core;
using Serilog.Events;

namespace Collector.Serilog.Enrichers.SensitiveInformation.DestructuringPolicies
{
    internal class SensitiveInformationTransformativeDestructuringPolicy<T> : IDestructuringPolicy
    {
        private readonly Func<T, object> _transform;

        public SensitiveInformationTransformativeDestructuringPolicy(Func<T, object> transform = null)
        {
            _transform = transform ?? throw new ArgumentException(nameof(transform));
        }

        public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventPropertyValue result)
        {
            if (value is T tValue)
            {
                result = propertyValueFactory.CreatePropertyValue(value: _transform(tValue), destructureObjects: true).AsSensitive();
                return true;
            }

            result = null;
            return false;
        }
    }
}