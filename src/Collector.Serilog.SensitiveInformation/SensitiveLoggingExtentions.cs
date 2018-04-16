using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using Collector.Serilog.SensitiveInformation.DestructuringPolicies;

using Serilog;
using Serilog.Configuration;

namespace Collector.Serilog.SensitiveInformation
{
    public static class SensitiveLoggingExtentions
    {
        internal const string SensitiveInformationHasBeenReviewed = SensitiveInformationEnricher.SensitiveInformation + "HasBeenReviewed";

        public static ILogger HasBeenReviewedForSensitiveInformation(this ILogger logger)
        {
            return logger.ForContext(SensitiveInformationHasBeenReviewed, true);
        }

        public static ILogger WithSensitiveInformation(this ILogger logger, SensitiveInformationType sensitiveInformationType, object value)
        {
            return logger.WithSensitiveInformation(sensitiveInformationType.ToString(), value);
        }

        public static ILogger WithSensitiveInformation(this ILogger logger, string propertyName, object value)
        {
            return logger.ForContext(propertyName, new Dictionary<string, object> { [SensitiveInformationEnricher.SensitiveInformation] = value }, destructureObjects: true);
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