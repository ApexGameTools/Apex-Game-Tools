/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Utilities
{
    using System;

    /// <summary>
    /// <para>Common validation routines.</para>
    /// </summary>
    public static class Ensure
    {
        /// <summary>
        /// <para>Check if the <paramref name="value"/> is <see langword="null"/> or an empty string.</para>
        /// </summary>
        /// <param name="value">
        /// <para>The value to check.</para>
        /// </param>
        /// <param name="variableName">
        /// <para>The name of the argument being checked.</para>
        /// </param>
        /// <exception cref="ArgumentException">
        /// If <pararef name="value"/> is <see langword="null"/> or empty.
        /// </exception>
        public static void ArgumentNotNullOrEmpty([ValidatedNotNullAttribute] string value, string variableName)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("Cannot be null or empty!", variableName);
            }
        }

        /// <summary>
        /// <para>Check if the <paramref name="value"/> is <see langword="null"/> (Nothing in Visual Basic).</para>
        /// </summary>
        /// <param name="value">
        /// <para>The value to check.</para>
        /// </param>
        /// <param name="variableName">
        /// <para>The name of the argument being checked.</para>
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <pararef name="value"/> is <see langword="null"/> (Nothing in Visual Basic).
        /// </exception>
        public static void ArgumentNotNull([ValidatedNotNullAttribute] object value, string variableName)
        {
            if (null == value)
            {
                throw new ArgumentNullException(variableName);
            }
        }

        /// <summary>
        /// Ensure that an argument is in range.
        /// </summary>
        /// <param name="check">The range check.</param>
        /// <param name="variableName">Name of the variable.</param>
        /// <param name="value">The value of the variable.</param>
        /// <param name="message">The message in case value is out of range.</param>
        /// <exception cref="System.ArgumentException">If value is out of range.</exception>
        public static void ArgumentInRange(Func<bool> check, string variableName, object value, string message = null)
        {
            if (!check())
            {
                throw new ArgumentException(message, variableName);
            }
        }

        /// <summary>
        /// Attribute to satisfy the CA1062 rule
        /// </summary>
        private sealed class ValidatedNotNullAttribute : Attribute
        {
        }
    }
}
