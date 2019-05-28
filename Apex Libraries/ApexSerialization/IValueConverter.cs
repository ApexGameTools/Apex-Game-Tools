/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Serialization
{
    using System;

    /// <summary>
    /// Interface for types that handle conversion to and from string representation of one or more types. Implementing classes must also have a parameterless constructor.
    /// </summary>
    public interface IValueConverter
    {
        /// <summary>
        /// Gets the types this converter can handle.
        /// </summary>
        /// <value>
        /// The handled types.
        /// </value>
        Type[] handledTypes { get; }

        /// <summary>
        /// Converts a value to its string representation.
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <returns>The string representation of the value.</returns>
        string ToString(object value);

        /// <summary>
        /// Converts a string to the type handled by this converter.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="targetType">Type to convert to.</param>
        /// <returns>The converted value.</returns>
        object FromString(string value, Type targetType);
    }
}
