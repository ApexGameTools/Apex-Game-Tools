﻿/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Serialization.Converters
{ 
    using System;
    using System.Globalization;

    /// <summary>
    /// Handles conversion of <see cref="double"/>s.
    /// </summary>
    /// <seealso cref="Apex.Serialization.IValueConverter" />
    public sealed class DoubleConverter : IValueConverter
    {
        /// <summary>
        /// Gets the types this converter can handle.
        /// </summary>
        /// <value>
        /// The handled types.
        /// </value>
        public Type[] handledTypes
        {
            get { return new[] { typeof(double) }; }
        }

        /// <summary>
        /// Converts a value to its string representation.
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <returns>The string representation of the value.</returns>
        public string ToString(object value)
        {
            return ((double)value).ToString("R", CultureInfo.InvariantCulture);
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
            return Convert.ToDouble(value, CultureInfo.InvariantCulture);
        }
    }
}
