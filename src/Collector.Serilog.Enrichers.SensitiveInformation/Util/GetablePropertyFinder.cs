using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace Collector.Serilog.Enrichers.SensitiveInformation.Util
{
    internal static class GetablePropertyFinder
    {
        private static readonly ConcurrentDictionary<Type, IReadOnlyCollection<PropertyInfo>> Cache = new ConcurrentDictionary<Type, IReadOnlyCollection<PropertyInfo>>();

        public static IReadOnlyCollection<PropertyInfo> GetAllProperties(this Type type)
        {
            return Cache.GetOrAdd(type, t => new ReadOnlyCollection<PropertyInfo>(GetAllPropertiesImpl(t).ToList()));
        }

        private static IEnumerable<PropertyInfo> GetAllPropertiesImpl(Type type)
        {
            var seenNames = new HashSet<string>();

            var currentTypeInfo = type.GetTypeInfo();

            while (currentTypeInfo.AsType() != typeof(object))
            {
                var unseenProperties = currentTypeInfo.DeclaredProperties.Where(p => p.CanRead &&
                                                                                     p.GetMethod.IsPublic &&
                                                                                     !p.GetMethod.IsStatic &&
                                                                                     (p.Name != "Item" || p.GetIndexParameters().Length == 0) &&
                                                                                     !seenNames.Contains(p.Name));

                foreach (var propertyInfo in unseenProperties)
                {
                    seenNames.Add(propertyInfo.Name);
                    yield return propertyInfo;
                }

                currentTypeInfo = currentTypeInfo.BaseType.GetTypeInfo();
            }
        }
    }
}