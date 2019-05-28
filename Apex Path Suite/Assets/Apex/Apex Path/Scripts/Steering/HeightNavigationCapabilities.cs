/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering
{
    using System;
    using Apex.Utilities;

    /// <summary>
    /// Represents the height navigation capabilities of a unit
    /// </summary>
    [Serializable]
    public struct HeightNavigationCapabilities
    {
        /// <summary>
        /// The maximum slope angle the unit can move up / down
        /// </summary>
        [MinCheck(0f)]
        public float maxSlopeAngle;

        /// <summary>
        /// The maximum climb height of the unit
        /// </summary>
        [MinCheck(0f)]
        public float maxClimbHeight;

        /// <summary>
        /// The maximum drop height of the unit
        /// </summary>
        [MinCheck(0f)]
        public float maxDropHeight;

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="lhs">The LHS.</param>
        /// <param name="rhs">The RHS.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(HeightNavigationCapabilities lhs, HeightNavigationCapabilities rhs)
        {
            return lhs.maxSlopeAngle.Equals(rhs.maxSlopeAngle) && lhs.maxClimbHeight.Equals(rhs.maxClimbHeight) && lhs.maxClimbHeight.Equals(rhs.maxClimbHeight);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="lhs">The LHS.</param>
        /// <param name="rhs">The RHS.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(HeightNavigationCapabilities lhs, HeightNavigationCapabilities rhs)
        {
            return !(lhs.maxSlopeAngle.Equals(rhs.maxSlopeAngle) && lhs.maxClimbHeight.Equals(rhs.maxClimbHeight) && lhs.maxClimbHeight.Equals(rhs.maxClimbHeight));
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return this.maxSlopeAngle.GetHashCode() ^ this.maxClimbHeight.GetHashCode() << 2 ^ this.maxDropHeight.GetHashCode() >> 2;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object other)
        {
            if (!(other is HeightNavigationCapabilities))
            {
                return false;
            }

            var rhs = (HeightNavigationCapabilities)other;
            return this.maxSlopeAngle.Equals(rhs.maxSlopeAngle) && this.maxClimbHeight.Equals(rhs.maxClimbHeight) && this.maxClimbHeight.Equals(rhs.maxClimbHeight);
        }
    }
}
