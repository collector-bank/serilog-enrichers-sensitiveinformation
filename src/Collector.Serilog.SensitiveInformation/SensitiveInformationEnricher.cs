
using System;
using System.Collections.Generic;
using System.Linq;

using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;

namespace Collector.Serilog.SensitiveInformation
{

    public class SensitiveInformationEnricher : ILogEventEnricher
    {
        internal const string SensitiveInformation = "__sensitiveInfo";

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var sensitiveProperties = logEvent.Properties
                                               .Select(p => CreateSensitiveProperty(p.Key, p.Value))
                                               .Where(p => p != null)
                                               .ToArray();
            if (!sensitiveProperties.Any())
                return;

            foreach (var property in logEvent.Properties.ToList())
            {
                var safeProperty = CreateSafeProperty(property.Key, property.Value);
                if (safeProperty == null)
                    logEvent.RemovePropertyIfPresent(property.Key);
                else
                    logEvent.AddOrUpdateProperty(safeProperty);
            }

            logEvent.AddOrUpdateProperty(new LogEventProperty(SensitiveInformation, new StructureValue(sensitiveProperties)));
        }

        public LogEventProperty CreateSensitiveProperty(string name, LogEventPropertyValue logEventPropertyValue)
        {
            var sensitivePropertyValue = CreateSensitivePropertyValue(logEventPropertyValue);

            return sensitivePropertyValue == null ? null : new LogEventProperty(name, sensitivePropertyValue);
        }

        private LogEventProperty CreateSafeProperty(string name, LogEventPropertyValue logEventPropertyValue)
        {
            var safePropertyValue = CreateSafePropertyValue(logEventPropertyValue);

            return safePropertyValue == null ? null : new LogEventProperty(name, safePropertyValue);
        }

        private LogEventPropertyValue CreateSensitivePropertyValue(LogEventPropertyValue logEventPropertyValue)
        {
            if (logEventPropertyValue is ScalarValue)
                return null;

            if (IsSensitiveValue(logEventPropertyValue))
                return GetSensitiveValue(logEventPropertyValue);

            return CreateEventPropertyValueRecursively(logEventPropertyValue, CreateSensitivePropertyValue);
        }

        private LogEventPropertyValue CreateSafePropertyValue(LogEventPropertyValue logEventPropertyValue)
        {
            if (logEventPropertyValue is ScalarValue)
                return logEventPropertyValue;

            if (IsSensitiveValue(logEventPropertyValue))
                return null;

            return CreateEventPropertyValueRecursively(logEventPropertyValue, CreateSafePropertyValue);
        }

        private bool IsSensitiveValue(LogEventPropertyValue logEventPropertyValue)
        {
            return logEventPropertyValue is DictionaryValue dictionaryValue && dictionaryValue.Elements.ContainsKey(new ScalarValue(SensitiveInformation));
        }

        private LogEventPropertyValue GetSensitiveValue(LogEventPropertyValue logEventPropertyValue)
        {
            var sensitiveValue = ((DictionaryValue)logEventPropertyValue).Elements.Single().Value;

            return CleanPropertyValueFromSensitivityMarkers(sensitiveValue);
        }

        private LogEventPropertyValue CleanPropertyValueFromSensitivityMarkers(LogEventPropertyValue logEventPropertyValue)
        {
            if (logEventPropertyValue is ScalarValue)
                return logEventPropertyValue;

            if (IsSensitiveValue(logEventPropertyValue))
                return GetSensitiveValue(logEventPropertyValue);

            return CreateEventPropertyValueRecursively(logEventPropertyValue, CleanPropertyValueFromSensitivityMarkers);
        }

        private static LogEventPropertyValue CreateEventPropertyValueRecursively(LogEventPropertyValue logEventPropertyValue, Func<LogEventPropertyValue, LogEventPropertyValue> recur)
        {
            if (logEventPropertyValue is SequenceValue sequenceValue)
            {
                var values = sequenceValue.Elements.Select(recur).Where(e => e != null).ToArray();
                return values.Any() ? new SequenceValue(values) : null;
            }

            if (logEventPropertyValue is DictionaryValue dictionaryValue)
            {
                var values = dictionaryValue.Elements
                                            .Select(kvp => new KeyValuePair<ScalarValue, LogEventPropertyValue>(kvp.Key, recur(kvp.Value)))
                                            .Where(kvp => kvp.Value != null)
                                            .ToArray();
                return values.Any() ? new DictionaryValue(values) : null;
            }

            if (logEventPropertyValue is StructureValue structureValue)
            {
                var values = structureValue.Properties
                                           .Select(p => new { p.Name, Value = recur(p.Value) })
                                           .Where(p => p.Value != null)
                                           .Select(p => new LogEventProperty(p.Name, p.Value))
                                           .ToArray();
                return values.Any() ? new StructureValue(values, typeTag: structureValue.TypeTag) : null;
            }

            SelfLog.WriteLine("Unknown LogEventPropertyValue type: {0}", logEventPropertyValue.GetType());
            return null;
        }
    }
}