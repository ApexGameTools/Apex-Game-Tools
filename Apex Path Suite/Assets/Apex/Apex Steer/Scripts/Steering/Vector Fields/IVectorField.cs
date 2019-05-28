/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Steering.VectorFields
{
    using Apex.PathFinding;
    using Apex.Units;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// Interface for objects that are vector fields.
    /// </summary>
    public interface IVectorField
    {
        /// <summary>
        /// Gets the group.
        /// </summary>
        TransientGroup<IUnitFacade> group { get; }

        /// <summary>
        /// Gets the current path.
        /// </summary>
        Path currentPath { get; }

        /// <summary>
        /// Gets the final destination - e.g. the last path node in the path.
        /// </summary>
        IPositioned destination { get; }

        /// <summary>
        /// Gets the next node position - e.g. the next path node in the path.
        /// </summary>
        IPositioned nextNodePosition { get; }

        /// <summary>
        /// Gets a value indicating whether this vector field is on final approach.
        /// </summary>
        /// <value>
        /// <c>true</c> if this vector field is on final approach; otherwise, <c>false</c>.
        /// </value>
        bool isOnFinalApproach { get; }

        /// <summary>
        /// Initializes this vector field (called right after being instantiated).
        /// </summary>
        void Initialize();

        /// <summary>
        /// Gets the vector field cell at a cell center position.
        /// Note the value passed as position MUST be a grid cell center.
        /// </summary>
        /// <param name="cell">The grid cell.</param>
        /// <returns>The vector field cell at the specified position.</returns>
        VectorFieldCell GetFieldCellAtPos(IGridCell cell);

        /// <summary>
        /// Sets a new path on this vector field.
        /// </summary>
        /// <param name="path">The path to set.</param>
        void SetNewPath(Path path);

        /// <summary>
        /// A visualization method called in OnDrawGizmos - only used for debugging.
        /// </summary>
        void DebugRender();
    }
}