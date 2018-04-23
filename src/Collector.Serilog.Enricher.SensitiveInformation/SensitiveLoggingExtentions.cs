using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using Collector.Serilog.Enricher.SensitiveInformation.DestructuringPolicies;
using Collector.Serilog.Enricher.SensitiveInformation.Util;

using Serilog;
using Serilog.Configuration;

namespace Collector.Serilog.Enricher.SensitiveInformation
{
    public static class SensitiveLoggingExtentions
    {
        public static ILogger MarkAsReviewedRegardingSensitiveInformation(this ILogger logger)
        {
            return logger.ForContext(Constants.SensitiveInformationHasBeenReviewed, true);
        }

        public static ILogger WithSensitiveInformation(this ILogger logger, SensitiveInformationType sensitiveInformationType, object value)
        {
            return logger.WithSensitiveInformation(sensitiveInformationType.ToString(), value);
        }

        public static ILogger WithSensitiveInformation(this ILogger logger, string propertyName, object value)
        {
            return logger.ForContext(propertyName, new Dictionary<string, object> { [Constants.SensitiveInformation] = value }, destructureObjects: true);
        }

        public static LoggerConfiguration AsSensitive<T>(this LoggerDestructuringConfiguration conf, bool stringify = true)
        {
            return conf.With(new SensitiveInformationDestructuringPolicy<T>(stringify));
        }

        public static LoggerConfiguration AsSensitiveByTransforming<T>(this LoggerDestructuringConfiguration conf, Func<T, object> transform)
        {
            return conf.With(new SensitiveInformationTransformativeDestructuringPolicy<T>(transform));
        }

        public static LoggerConfiguration HasSensitiveProperties<T>(this LoggerDestructuringConfiguration conf, params Expression<Func<T, object>>[] sensitiveProperties)
        {
            return conf.With(new SensitiveInformationPropertyMarkingDestructuringPolicy<T>(sensitiveProperties));
        }
    }
}