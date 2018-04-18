
using System;
using System.Collections.Generic;
using System.Linq;

using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;

using Constants = Collector.Serilog.SensitiveInformation.Util.Constants;

namespace Collector.Serilog.SensitiveInformation
{
    public class SensitiveInformationEnricher : ILogEventEnricher
    {
        private readonly HashSet<string> _blacklistedPropertyNames = new HashSet<string>();
        private static readonly ScalarValue SensitiveInformationScalarValue = new ScalarValue(Constants.SensitiveInformation);

        public SensitiveInformationEnricher(params string[] blacklistedPropertyNames)
        {
            if (blacklistedPropertyNames == null)
                throw new ArgumentNullException(nameof(blacklistedPropertyNames));

            if(blacklistedPropertyNames.Contains(null))
                throw new ArgumentException("Blacklist can not contain null entries", nameof(blacklistedPropertyNames));

            foreach (var sensitiveInformationType in Enum.GetValues(typeof(SensitiveInformationType)).OfType<SensitiveInformationType>())
            {
                _blacklistedPropertyNames.Add(sensitiveInformationType.ToString());
            }
            foreach (var propertyName in blacklistedPropertyNames)
            {
                _blacklistedPropertyNames.Add(propertyName);
            }
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var sensitiveProperties = logEvent.Properties
                                              .Select(p => CreateSensitiveProperty(p.Key, p.Value))
                                              .Where(p => p != null)
                                              .ToDictionary(p => p.Name);
            if (!sensitiveProperties.Any())
                return;

            foreach (var property in logEvent.Properties.Where(p => sensitiveProperties.ContainsKey(p.Key)).ToList())
            {
                var safeProperty = CreateSafeProperty(property.Key, property.Value);
                if (safeProperty == null)
                    logEvent.RemovePropertyIfPresent(property.Key);
                else
                    logEvent.AddOrUpdateProperty(safeProperty);
            }

            logEvent.AddOrUpdateProperty(new LogEventProperty(Constants.SensitiveInformation, new StructureValue(sensitiveProperties.Values)));
        }

        public LogEventProperty CreateSensitiveProperty(string name, LogEventPropertyValue logEventPropertyValue)
        {
            var sensitivePropertyValue = CreateSensitivePropertyValue(name, logEventPropertyValue);

            return sensitivePropertyValue == null ? null : new LogEventProperty(name, sensitivePropertyValue);
        }

        private LogEventProperty CreateSafeProperty(string name, LogEventPropertyValue logEventPropertyValue)
        {
            var safePropertyValue = CreateSafePropertyValue(name, logEventPropertyValue);

            return safePropertyValue == null ? null : new LogEventProperty(name, safePropertyValue);
        }

        private LogEventPropertyValue CreateSensitivePropertyValue(string name, LogEventPropertyValue logEventPropertyValue)
        {
            if (_blacklistedPropertyNames.Contains(name) || IsSensitiveValue(logEventPropertyValue))
                return CleanPropertyValueFromSensitivityMarkers(name, logEventPropertyValue);

            if (logEventPropertyValue is ScalarValue)
                return null;

            return CreateEventPropertyValueRecursively(logEventPropertyValue, CreateSensitivePropertyValue);
        }

        private LogEventPropertyValue CreateSafePropertyValue(string name, LogEventPropertyValue logEventPropertyValue)
        {
            if (_blacklistedPropertyNames.Contains(name) || IsSensitiveValue(logEventPropertyValue))
                return null;

            if (logEventPropertyValue is ScalarValue)
                return logEventPropertyValue;

            return CreateEventPropertyValueRecursively(logEventPropertyValue, CreateSafePropertyValue);
        }

        private bool IsSensitiveValue(LogEventPropertyValue logEventPropertyValue)
        {
            return logEventPropertyValue is DictionaryValue dictionaryValue && dictionaryValue.Elements.ContainsKey(SensitiveInformationScalarValue);
        }

        private LogEventPropertyValue GetSensitiveValue(string name, LogEventPropertyValue logEventPropertyValue)
        {
            var sensitiveValue = ((DictionaryValue)logEventPropertyValue).Elements.Single().Value;

            return CleanPropertyValueFromSensitivityMarkers(name, sensitiveValue);
        }

        private LogEventPropertyValue CleanPropertyValueFromSensitivityMarkers(string name, LogEventPropertyValue logEventPropertyValue)
        {
            if (logEventPropertyValue is ScalarValue)
                return logEventPropertyValue;

            if (IsSensitiveValue(logEventPropertyValue))
                return GetSensitiveValue(name, logEventPropertyValue);

            return CreateEventPropertyValueRecursively(logEventPropertyValue, CleanPropertyValueFromSensitivityMarkers);
        }

        private static LogEventPropertyValue CreateEventPropertyValueRecursively(LogEventPropertyValue logEventPropertyValue, Func<string, LogEventPropertyValue, LogEventPropertyValue> recur)
        {
            switch (logEventPropertyValue)
            {
                case SequenceValue sequenceValue:
                {
                    var values = sequenceValue.Elements
                                              .Select(e => recur(null, e))
                                              .Where(e => e != null)
                                              .ToArray();
                    return values.Any() ? new SequenceValue(values) : null;
                }
                case DictionaryValue dictionaryValue:
                {
                    var values = dictionaryValue.Elements
                                                .ToDictionary(kvp => kvp.Key, kvp => recur(kvp.Key.Value?.ToString(), kvp.Value))
                                                .Where(kvp => kvp.Value != null)
                                                .ToArray();
                    return values.Any() ? new DictionaryValue(values) : null;
                }
                case StructureValue structureValue:
                {
                    var values = structureValue.Properties
                                               .Select(p => new { p.Name, Value = recur(p.Name, p.Value) })
                                               .Where(p => p.Value != null)
                                               .Select(p => new LogEventProperty(p.Name, p.Value))
                                               .ToArray();
                    return values.Any() ? new StructureValue(values, typeTag: structureValue.TypeTag) : null;
                }
                default:
                    SelfLog.WriteLine("Unknown LogEventPropertyValue type: {0}", logEventPropertyValue.GetType());
                    return null;
            }
        }
    }
}