/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.DataStructures
{
    /// <summary>
    /// A vector in the XZ plane.
    /// </summary>
    public struct VectorXZ
    {
        /// <summary>
        /// The x coordinate
        /// </summary>
        public int x;

        /// <summary>
        /// The z coordinate
        /// </summary>
        public int z;

        /// <summary>
        /// Initializes a new instance of the <see cref="VectorXZ"/> struct.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="z">The z coordinate.</param>
        public VectorXZ(int x, int z)
        {
            this.x = x;
            this.z = z;
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="lhs">The LHS.</param>
        /// <param name="rhs">The RHS.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(VectorXZ lhs, VectorXZ rhs)
        {
            if (lhs.x != rhs.x)
            {
                return false;
            }

            return lhs.z == rhs.z;
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="lhs">The LHS.</param>
        /// <param name="rhs">The RHS.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(VectorXZ lhs, VectorXZ rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        /// Implements the operator +.
        /// </summary>
        /// <param name="a">The first vector</param>
        /// <param name="b">The second vector.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static VectorXZ operator +(VectorXZ a, VectorXZ b)
        {
            return new VectorXZ(a.x + b.x, a.z + b.z);
        }

        /// <summary>
        /// Implements the operator -.
        /// </summary>
        /// <param name="a">The first vector</param>
        /// <param name="b">The second vector.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static VectorXZ operator -(VectorXZ a, VectorXZ b)
        {
            return new VectorXZ(a.x - b.x, a.z - b.z);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return this.x.GetHashCode() ^ this.z.GetHashCode();
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
            if (!(other is VectorXZ))
            {
                return false;
            }

            var rhs = (VectorXZ)other;
            if (this.x != rhs.x)
            {
                return false;
            }

            return this.z == rhs.z;
        }
    }
}
