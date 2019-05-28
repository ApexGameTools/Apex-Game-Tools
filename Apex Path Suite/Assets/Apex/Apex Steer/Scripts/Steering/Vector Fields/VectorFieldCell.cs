/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Steering.VectorFields
{
    using Apex.DataStructures;

    /// <summary>
    /// A vector field cell - e.g., a cell used by <see cref="IVectorField"/> types.
    /// Includes a vector (2D <see cref="PlaneVector"/>).
    /// Also has an integer pointing to an index in the current path, which holds a portal node (otherwise -1).
    /// </summary>
    public struct VectorFieldCell
    {
        /// <summary>
        /// The direction of the steering vector
        /// </summary>
        public PlaneVector direction;

        /// <summary>
        /// Next index in the path where a portal exists
        /// </summary>
        public int pathPortalIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="VectorFieldCell"/> struct.
        /// </summary>
        /// <param name="direction">The direction.</param>
        /// <param name="portalIndex">Index of the portal.</param>
        public VectorFieldCell(PlaneVector direction, int portalIndex)
        {
            this.direction = direction;
            this.pathPortalIndex = portalIndex;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VectorFieldCell"/> struct.
        /// </summary>
        /// <param name="direction">The direction.</param>
        /// The path portal index defaults to -1 in this case.
        public VectorFieldCell(PlaneVector direction)
        {
            this.direction = direction;
            this.pathPortalIndex = -1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VectorFieldCell"/> struct.
        /// </summary>
        /// <param name="portalIndex">Index of the portal.</param>
        /// The plane vector defaults to a PlaneVector.zero (0f, 0f) in this case.
        public VectorFieldCell(int portalIndex)
        {
            this.direction = PlaneVector.zero;
            this.pathPortalIndex = portalIndex;
        }
    }
}