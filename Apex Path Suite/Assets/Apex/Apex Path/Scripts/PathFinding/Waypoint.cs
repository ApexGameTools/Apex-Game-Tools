/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.PathFinding
{
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// Represents a waypoint position in world space
    /// </summary>
    public struct Waypoint : IPositioned
    {
        private Vector3 _pos;

        /// <summary>
        /// Initializes a new instance of the <see cref="Waypoint"/> struct.
        /// </summary>
        /// <param name="pos">The position.</param>
        public Waypoint(Vector3 pos)
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
        /// Performs an implicit conversion from <see cref="Waypoint"/> to <see cref="Vector3"/>.
        /// </summary>
        /// <param name="pos">The position.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator Vector3(Waypoint pos)
        {
            return pos.position;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="Vector3"/> to <see cref="Waypoint"/>.
        /// </summary>
        /// <param name="pos">The position.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator Waypoint(Vector3 pos)
        {
            return new Waypoint(pos);
        }
    }
}