using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Collector.Serilog.Enrichers.SensitiveInformation.Util;

using Serilog.Core;
using Serilog.Events;

namespace Collector.Serilog.Enrichers.SensitiveInformation.DestructuringPolicies
{
    internal class SensitiveInformationPropertyMarkingDestructuringPolicy<T> : IDestructuringPolicy
    {
        private readonly bool _shouldContain;
        private readonly ISet<string> _propertyNames;

        public SensitiveInformationPropertyMarkingDestructuringPolicy(bool shouldContain, params Expression<Func<T, object>>[] properties)
        {
            _shouldContain = shouldContain;
            _propertyNames = new HashSet<string>(properties.Select(e => e.Body).Cast<MemberExpression>().Select(m => m.Member.Name));
        }

        public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventPropertyValue result)
        {
            if (value is T)
            {
                var properties = value.GetType().GetAllProperties();

                var logEventProperties = new List<LogEventProperty>();

                foreach (var propertyInfo in properties)
                {
                    var propValue = propertyValueFactory.CreatePropertyValue(propertyInfo.GetValue(value), destructureObjects: true);
                    
                    if (_propertyNames.Contains(propertyInfo.Name) == _shouldContain)
                        propValue = propValue.AsSensitive();

                    logEventProperties.Add(new LogEventProperty(propertyInfo.Name, propValue));
                }

                result = new StructureValue(logEventProperties);
                return true;
            }

            result = null;
            return false;
        }
    }
}