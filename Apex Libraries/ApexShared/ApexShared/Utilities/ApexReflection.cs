/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Reflection helpers related to Apex relevant types
    /// </summary>
    public static class ApexReflection
    {
        /// <summary>
        /// Gets the Apex relevant types, meaning all types in either the Assembly-CSharp (or similar) or assemblies marked with the <see cref="ApexRelevantAssemblyAttribute"/>
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Type> GetRelevantTypes()
        {
            return from a in AppDomain.CurrentDomain.GetAssemblies()
                   where a.FullName.IndexOf("Assembly-CSharp", StringComparison.OrdinalIgnoreCase) >= 0 || a.IsDefined(typeof(ApexRelevantAssemblyAttribute), false)
                   from t in a.GetTypes()
                   select t;
        }

        /// <summary>
        /// Gets the proper name of a type. This is specifically for generic types to show their generic arguments as part of the type name.
        /// </summary>
        /// <param name="t">The type.</param>
        /// <returns>The name including any generic arguments.</returns>
        public static string ProperName(this Type t, bool fullName)
        {
            if (!t.IsGenericType)
            {
                return fullName ? t.FullName : t.Name;
            }

            StringBuilder b = new StringBuilder();
            ProperName(t, fullName, b);
            return b.ToString();
        }

        private static void ProperName(Type t, bool fullName, StringBuilder b)
        {
            string baseName = fullName ? t.FullName : t.Name;
            if (!t.IsGenericType)
            {
                b.Append(baseName);
                return;
            }

            Type[] typeArguments = t.GetGenericArguments();

            baseName = baseName.Substring(0, baseName.IndexOf('`'));
            b.Append(baseName);
            b.Append('<');

            if (t.IsGenericTypeDefinition)
            {
                b.Append(',', typeArguments.Length);
            }
            else
            {
                ProperName(typeArguments[0], fullName, b);
                for (int i = 1; i < typeArguments.Length; i++)
                {
                    b.Append(',');
                    ProperName(typeArguments[i], fullName, b);
                }
            }

            b.Append('>');
        }
    }
}
