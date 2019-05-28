/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Serialization.Converters
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Handles conversion of primitive types, e.g. <see cref="Int32"/>, <see cref="Boolean"/> etc.
    /// </summary>
    /// <seealso cref="Apex.Serialization.IValueConverter" />
    public sealed class PrimitivesConverter : IValueConverter
    {
        /// <summary>
        /// Gets the types this converter can handle.
        /// </summary>
        /// <value>
        /// The handled types.
        /// </value>
        public Type[] handledTypes
        {
            get { return new[] { typeof(byte), typeof(short), typeof(int), typeof(long), typeof(ushort), typeof(uint), typeof(ulong) }; }
        }

        /// <summary>
        /// Converts a value to its string representation.
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <returns>The string representation of the value.</returns>
        public string ToString(object value)
        {
            return ((IConvertible)value).ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts a string to the type handled by this converter.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="targetType">Type to convert to.</param>
        /// <returns>
        /// The converted value.
        /// </returns>
        public object FromString(string value, Type targetType)
        {
            return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
        }
    }
}
