namespace Apex.Utilities
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents an empty structure of a given type.
    /// </summary>
    /// <typeparam name="T">The type</typeparam>
    public static class Empty<T>
    {
        /// <summary>
        /// An empty array
        /// </summary>
        public static readonly T[] array = new T[0];

        public static readonly IList<T> list = new List<T>(0).AsReadOnly();
    }
}
