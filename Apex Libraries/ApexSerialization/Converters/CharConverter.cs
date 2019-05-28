/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Serialization.Converters
{ 
    using System;
    using System.Globalization;

    /// <summary>
    /// Handles conversion of <see cref="char"/>s.
    /// </summary>
    /// <seealso cref="Apex.Serialization.IValueConverter" />
    public sealed class CharConverter : IValueConverter
    {
        /// <summary>
        /// Gets the types this converter can handle.
        /// </summary>
        /// <value>
        /// The handled types.
        /// </value>
        public Type[] handledTypes
        {
            get { return new[] { typeof(char) }; }
        }

        /// <summary>
        /// Converts a value to its string representation.
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <returns>The string representation of the value.</returns>
        public string ToString(object value)
        {
            var stringVal = ((char)value).ToString(CultureInfo.InvariantCulture).TrimStart('\0');
            if (string.IsNullOrEmpty(stringVal))
            {
                return string.Empty;
            }

            return stringVal;
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
            if (string.IsNullOrEmpty(value))
            {
                return '\0';
            }

            return Convert.ToChar(value, CultureInfo.InvariantCulture);
        }
    }
}
