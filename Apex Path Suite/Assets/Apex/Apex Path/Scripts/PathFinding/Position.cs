/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.PathFinding
{
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// Represents a position in world space
    /// </summary>
    public struct Position : IPositioned
    {
        private Vector3 _pos;

        /// <summary>
        /// Initializes a new instance of the <see cref="Position"/> struct.
        /// </summary>
        /// <param name="pos">The position.</param>
        public Position(Vector3 pos)
        {
            _pos = pos;
        }

        /// <summary>
        /// Gets the position vector.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        public Vector3 position
        {
            get { return _pos; }
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="Position"/> to <see cref="Vector3"/>.
        /// </summary>
        /// <param name="pos">The position.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator Vector3(Position pos)
        {
            return pos.position;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="Vector3"/> to <see cref="Position"/>.
        /// </summary>
        /// <param name="pos">The position.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator Position(Vector3 pos)
        {
            return new Position(pos);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "Position: " + _pos.ToString();
        }
    }
}