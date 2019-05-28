/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Utilities
{
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// General utilities of various sorts
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Copies the public properties from one class instance to another. Only properties present on both types will be copied (obviously).
        /// </summary>
        /// <param name="from">The instance to copy from.</param>
        /// <param name="to">The instance to copy to.</param>
        public static void CopyProps(object from, object to)
        {
            Ensure.ArgumentNotNull(from, "from");
            Ensure.ArgumentNotNull(to, "to");
#if NETFX_CORE
            var fromType = from.GetType().GetTypeInfo();
            var toType = to.GetType().GetTypeInfo();

            //Process properties
            var fromProps = from p in fromType.DeclaredProperties
                        where p.CanRead
                        select p;

            var toProps = (from p in toType.DeclaredProperties
                        where p.CanWrite
                        select p).ToDictionary(p => p.Name);
#else
            var fromType = from.GetType();
            var toType = to.GetType();

            //Process properties
            var fromProps = from p in fromType.GetProperties()
                            where p.CanRead
                            select p;

            var toProps = (from p in toType.GetProperties()
                           where p.CanWrite
                           select p).ToDictionary(p => p.Name);
#endif
            foreach (var p in fromProps)
            {
                PropertyInfo toProp;
                if (toProps.TryGetValue(p.Name, out toProp))
                {
                    toProp.SetValue(to, p.GetValue(from, null), null);
                }
            }
        }
    }
}
