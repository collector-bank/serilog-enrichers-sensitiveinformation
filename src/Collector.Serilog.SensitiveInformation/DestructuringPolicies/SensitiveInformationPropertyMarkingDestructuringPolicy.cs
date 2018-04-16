﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Collector.Serilog.SensitiveInformation.Util;

using Serilog.Core;
using Serilog.Events;

namespace Collector.Serilog.SensitiveInformation.DestructuringPolicies
{
    public class SensitiveInformationPropertyMarkingDestructuringPolicy<T> : IDestructuringPolicy
    {
        private readonly ISet<string> _sensitiveProperties;

        public SensitiveInformationPropertyMarkingDestructuringPolicy(params Expression<Func<T, object>>[] sensitiveProperties)
        {
            _sensitiveProperties = new HashSet<string>(sensitiveProperties.Select(e => e.Body).Cast<MemberExpression>().Select(m => m.Member.Name));
        }

        public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventPropertyValue result)
        {
            if (value is T)
            {
                var properties = value.GetType().GetAllProperties();

                var dic = new Dictionary<ScalarValue, LogEventPropertyValue>();

                foreach (var propertyInfo in properties)
                {
                    var propValue = propertyValueFactory.CreatePropertyValue(propertyInfo.GetValue(value), destructureObjects: true);
                    
                    if (_sensitiveProperties.Contains(propertyInfo.Name))
                        propValue = propValue.AsSensitive();

                    dic[new ScalarValue(propertyInfo.Name)] = propValue;
                }

                result = new DictionaryValue(dic);
                return true;
            }

            result = null;
            return false;
        }
    }
}