﻿using System;
using System.Linq;
using System.Reflection;

namespace FlurlGraphQL.ReflectionExtensions
{
    internal static class ReflectionExtensions
    {
        /// <summary>
        /// BBernard
        /// A wonderful little utility for robust Generic Type comparisons 
        /// Taken from GraphQL Extensions Open Source library here:
        /// https://github.com/cajuncoding/GraphQL.RepoDB/blob/main/GraphQL.ResolverProcessingExtensions/DotNetCustomExtensions/TypeCustomExtensions.cs
        /// Also For more info see: https://stackoverflow.com/a/37184228/7293142
        ///  </summary>
        /// <param name="type"></param>
        /// <param name="parentType"></param>
        /// <returns></returns>
        public static bool IsDerivedFromGenericParent(this Type type, Type parentType)
        {
            if (!parentType.IsGenericType)
            {
                throw new ArgumentException("type must be generic", nameof(parentType));
            }
            else if (type == null || type == typeof(object))
            {
                return false;
            }
            else if ((type.IsGenericType && type.GetGenericTypeDefinition() == parentType)
                     || type.BaseType.IsDerivedFromGenericParent(parentType)
                     //Recursively search for Interfaces...
                     || type.GetInterfaces().Any(t => t.IsDerivedFromGenericParent(parentType))
            )
            {
                return true;
            }

            return false;
        }

        public static bool InheritsFrom<TInterface>(this Type type)
            => typeof(TInterface).IsAssignableFrom(type);

        public static bool InheritsFrom(this Type type, Type baseType)
            => baseType.IsAssignableFrom(type);

        /// <summary>
        /// BBernard
        /// Convenience method for getting a private/protected Property Value of an object instance with brute force reflection...
        /// NOTE: This is not the highest performance mechanism for doing this because Reflection is always used and the MethodInfo is not cached!
        ///         Therefore care should be taken to avoid using it in a tight loop.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T BruteForceGet<T>(this object obj, string name)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            Type type = obj.GetType();
            FieldInfo field = type.GetField(name, flags);
            if (field != null)
                return (T)field.GetValue(obj);

            PropertyInfo property = type.GetProperty(name, flags);
            if (property != null)
                return (T)property.GetValue(obj, null);

            return default;
        }

        public static Type FindType(this AppDomain appDomain, string className, string assemblyName = null, string namespaceName = null)
            => appDomain.GetAssemblies()
                .Where(a => assemblyName == null || (a.GetName().Name?.Equals(assemblyName, StringComparison.OrdinalIgnoreCase) ?? false))
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t =>
                    t.Namespace != null
                    && (namespaceName == null || t.Namespace.Equals(namespaceName, StringComparison.OrdinalIgnoreCase))
                    && t.Name.Equals(className, StringComparison.OrdinalIgnoreCase)
                );
    }
}
