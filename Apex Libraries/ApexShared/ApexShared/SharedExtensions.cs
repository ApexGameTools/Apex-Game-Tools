namespace Apex
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Apex.DataStructures;

    /// <summary>
    /// Various extension of common nature.
    /// </summary>
    public static class SharedExtensions
    {
        /// <summary>
        /// Applies the specified action to all elements in a list.
        /// </summary>
        /// <typeparam name="T">The item type</typeparam>
        /// <param name="list">The list.</param>
        /// <param name="action">The action.</param>
        public static void Apply<T>(this IEnumerable<T> list, Action<T> action)
        {
            var iter = list.GetEnumerator();
            while (iter.MoveNext())
            {
                action(iter.Current);
            }
        }

        /// <summary>
        /// Applies the specified action to all elements in a list.
        /// </summary>
        /// <typeparam name="T">The item type</typeparam>
        /// <param name="list">The list.</param>
        /// <param name="action">The action.</param>
        public static void Apply<T>(this IList<T> list, Action<T> action)
        {
            var count = list.Count;
            for (int i = 0; i < count; i++)
            {
                action(list[i]);
            }
        }

        /// <summary>
        /// Applies the specified action to all elements in a list.
        /// </summary>
        /// <typeparam name="T">The item type</typeparam>
        /// <param name="list">The list.</param>
        /// <param name="action">The action.</param>
        public static void Apply<T>(this IIndexable<T> list, Action<T> action)
        {
            var count = list.count;
            for (int i = 0; i < count; i++)
            {
                action(list[i]);
            }
        }

        /// <summary>
        /// Copies the contents of the list to a new array and returns it.
        /// </summary>
        /// <typeparam name="T">The type of the list items</typeparam>
        /// <param name="list">The list.</param>
        /// <returns>The new array containing the items of the list</returns>
        public static T[] ToArray<T>(this IIndexable<T> list)
        {
            var count = list.count;
            var arr = new T[count];
            for (int i = 0; i < count; i++)
            {
                arr[i] = list[i];
            }

            return arr;
        }

        /// <summary>
        /// Adds the range of elements.
        /// </summary>
        /// <typeparam name="T">The item type</typeparam>
        /// <param name="list">The list.</param>
        /// <param name="items">The items.</param>
        public static void AddRange<T>(this ICollection<T> list, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                list.Add(item);
            }
        }

        /// <summary>
        /// Utility enhancement to ordinary lists to allow them to work as a set. Only use this on small lists otherwise a dedicated data structure should be employed.
        /// </summary>
        /// <typeparam name="T">The item type</typeparam>
        /// <param name="list">The list.</param>
        /// <param name="item">The item.</param>
        public static bool AddUnique<T>(this IList<T> list, T item)
        {
            if (!list.Contains(item))
            {
                list.Add(item);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Utility enhancement to ordinary lists to allow them to work as a set. Only use this on small lists otherwise a dedicated data structure should be employed.
        /// </summary>
        /// <typeparam name="T">The item type</typeparam>
        /// <param name="list">The list.</param>
        /// <param name="items">The items.</param>
        public static void AddRangeUnique<T>(this IList<T> list, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                list.AddUnique(item);
            }
        }

        /// <summary>
        /// Repositions a list element at a new index.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="fromIdx">From index.</param>
        /// <param name="toIdx">To index.</param>
        public static void Reorder(this IList list, int fromIdx, int toIdx)
        {
            var item = list[fromIdx];

            if (fromIdx < toIdx)
            {
                for (int i = fromIdx + 1; i <= toIdx; i++)
                {
                    list[i - 1] = list[i];
                }
            }
            else
            {
                for (int i = fromIdx - 1; i >= toIdx; i--)
                {
                    list[i + 1] = list[i];
                }
            }

            list[toIdx] = item;
        }

        /// <summary>
        /// Repositions a list element at a new index.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="fromIdx">From index.</param>
        /// <param name="toIdx">To index.</param>
        public static void ReorderList<T>(this IList<T> list, int fromIdx, int toIdx)
        {
            var item = list[fromIdx];

            if (fromIdx < toIdx)
            {
                for (int i = fromIdx + 1; i <= toIdx; i++)
                {
                    list[i - 1] = list[i];
                }
            }
            else
            {
                for (int i = fromIdx - 1; i >= toIdx; i--)
                {
                    list[i + 1] = list[i];
                }
            }

            list[toIdx] = item;
        }

        /// <summary>
        /// Ensures the capacity of a List.
        /// </summary>
        /// <typeparam name="T">The list item type</typeparam>
        /// <param name="list">The list.</param>
        /// <param name="capacity">The capacity to sensure.</param>
        public static void EnsureCapacity<T>(this List<T> list, int capacity)
        {
            if (list.Capacity >= capacity)
            {
                return;
            }

            list.Capacity = capacity;
        }

        /// <summary>
        /// Gets an attribute if it exists.
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <param name="inf">The element on which to look for the attribute.</param>
        /// <param name="inherit">Whether or not to include inherited attributes.</param>
        /// <returns>The attribute instance if found; otherwise null.</returns>
        public static T GetAttribute<T>(this ICustomAttributeProvider inf, bool inherit) where T : Attribute
        {
            if (inf == null)
            {
                return null;
            }

            var attribs = inf.GetCustomAttributes(typeof(T), inherit);
            if (attribs == null || attribs.Length == 0)
            {
                return null;
            }

            return (T)attribs[0];
        }

        /// <summary>
        /// Gets all attributes of a given type on an element, e.g. type, property etc.
        /// </summary>
        /// <typeparam name="T">The attribute type</typeparam>
        /// <param name="inf">The element on which to look for the attribute.</param>
        /// <param name="inherit">Whether or not to include inherited attributes.</param>
        /// <returns>An enumerable of all attribute instances, or null if none were found.</returns>
        public static IEnumerable<T> GetAttributes<T>(this ICustomAttributeProvider inf, bool inherit) where T : Attribute
        {
            if (inf == null)
            {
                return null;
            }

            var attribs = inf.GetCustomAttributes(typeof(T), inherit);
            if (attribs == null)
            {
                return null;
            }

            return attribs.Cast<T>();
        }

        /// <summary>
        /// Determines whether the specified attribute is defined.
        /// </summary>
        /// <typeparam name="T">The attribute type</typeparam>
        /// <param name="inf">The element on which to look for the attribute.</param>
        /// <param name="inherit">Whether or not to include inherited attributes.</param>
        /// <returns><c>true</c> if found; otherwise <c>false</c>.</returns>
        public static bool IsDefined<T>(this ICustomAttributeProvider inf, bool inherit) where T : Attribute
        {
            if (inf == null)
            {
                return false;
            }

            return inf.IsDefined(typeof(T), inherit);
        }

        /// <summary>
        /// Gets a value from a dictionary or null if the key was not found.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="dict">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <returns>The value if found; otherwise <c>null</c></returns>
        public static T Value<TKey, T>(this Dictionary<TKey, T> dict, TKey key)
        {
            T val;
            if (dict.TryGetValue(key, out val))
            {
                return val;
            }

            return default(T);
        }

        /// <summary>
        /// Inserts spaces between each token in a Pascal cased string
        /// </summary>
        /// <param name="pascalString">The string to parse</param>
        /// <returns>The converted string</returns>
        public static string ExpandFromPascal(this string pascalString)
        {
            if (pascalString == null)
            {
                return null;
            }

            var transformer = new StringBuilder(pascalString.Length);

            pascalString = pascalString.TrimStart('_');
            var length = pascalString.Length;

            if (length > 0)
            {
                transformer.Append(char.ToUpper(pascalString[0]));
                for (int i = 1; i < length; i++)
                {
                    if (char.IsUpper(pascalString, i) && (i + 1 < length) && (!char.IsUpper(pascalString, i - 1) || !char.IsUpper(pascalString, i + 1)))
                    {
                        transformer.Append(" ");
                    }

                    transformer.Append(pascalString[i]);
                }
            }

            return transformer.ToString();
        }

        /// <summary>
        /// Prettyfies the name inserting spaces and removing any generics from the name.
        /// </summary>
        /// <param name="t">The type.</param>
        /// <returns>The oh so pretty name.</returns>
        public static string PrettyName(this Type t)
        {
            var typeName = t.IsGenericType ? t.Name.Substring(0, t.Name.IndexOf('`')) : t.Name;

            return ExpandFromPascal(typeName);
        }

        /// <summary>
        /// Fuses the specified arrays.
        /// </summary>
        /// <typeparam name="T">The type of the arrays</typeparam>
        /// <param name="arrOne">The first array.</param>
        /// <param name="arrTwo">The second array.</param>
        /// <returns>An array containing all elements of the two source arrays in their original order. If either array is null the other is returned.</returns>
        public static T[] Fuse<T>(this T[] arrOne, T[] arrTwo)
        {
            if (arrOne == null)
            {
                return arrTwo;
            }

            if (arrTwo == null)
            {
                return arrOne;
            }

            var newArr = new T[arrOne.Length + arrTwo.Length];
            Array.Copy(arrOne, newArr, arrOne.Length);
            Array.Copy(arrTwo, 0, newArr, arrOne.Length, arrTwo.Length);

            return newArr;
        }

        /// <summary>
        /// Gets the index of a value in an array.
        /// </summary>
        /// <typeparam name="T">The type of items in the array</typeparam>
        /// <param name="array">The array.</param>
        /// <param name="value">The value to look for.</param>
        /// <returns>The index of the value, or -1 if not found</returns>
        public static int IndexOf<T>(this T[] array, T value) where T : IEquatable<T>
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].Equals(value))
                {
                    return i;
                }
            }

            return -1;
        }

        public static string SafeTrim(this string s)
        {
            if (s == null)
            {
                return null;
            }

            return s.Trim();
        }
    }
}
