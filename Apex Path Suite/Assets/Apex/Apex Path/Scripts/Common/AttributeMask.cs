/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Common
{
    using System;

    /// <summary>
    /// Represent a mask (combination of) attributes.
    /// </summary>
    public struct AttributeMask : IEquatable<AttributeMask>
    {
        /// <summary>
        /// Attribute mask representing no attributes
        /// </summary>
        public static readonly AttributeMask None = 0;

        /// <summary>
        /// Attribute mask representing all attributes
        /// </summary>
        public static readonly AttributeMask All = ~0;

        /// <summary>
        /// The value
        /// </summary>
        public int value;

        /// <summary>
        /// Performs an implicit conversion from <see cref="AttributeMask"/> to <see cref="System.Int32"/>.
        /// </summary>
        /// <param name="mask">The mask.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator int(AttributeMask mask)
        {
            return mask.value;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.Int32"/> to <see cref="AttributeMask"/>.
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator AttributeMask(int val)
        {
            return new AttributeMask
            {
                value = val
            };
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="me">Me.</param>
        /// <param name="other">The other.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(AttributeMask me, AttributeMask other)
        {
            return me.value == other.value;
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="me">Me.</param>
        /// <param name="other">The other.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(AttributeMask me, AttributeMask other)
        {
            return me.value != other.value;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public bool Equals(AttributeMask other)
        {
            return this == other;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (!(obj is AttributeMask))
            {
                return false;
            }

            return ((AttributeMask)obj).value == this.value;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return this.value.GetHashCode();
        }
    }
}
