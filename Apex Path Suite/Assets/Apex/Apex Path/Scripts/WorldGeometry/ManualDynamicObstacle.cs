/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    using Apex.Common;
    using Apex.DataStructures;
    using Apex.Utilities;
    using UnityEngine;

    /// <summary>
    /// A dynamic obstacle that can be instantiated on the fly. It is activated / deactivated manually.
    /// </summary>
    public partial class ManualDynamicObstacle : IDynamicObstacle
    {
        private bool _active = false;
        private Bounds _bounds;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManualDynamicObstacle"/> class.
        /// </summary>
        /// <param name="coverage">The coverage area.</param>
        public ManualDynamicObstacle(Bounds coverage)
            : this(coverage, AttributeMask.None)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManualDynamicObstacle"/> class.
        /// </summary>
        /// <param name="coverage">The coverage area.</param>
        /// <param name="exceptions">The exceptions mask controlling which units ignore this obstacle.</param>
        public ManualDynamicObstacle(Bounds coverage, AttributeMask exceptions)
        {
            _bounds = coverage;
            this.exceptionsMask = exceptions;
        }

        /// <summary>
        /// Gets the position.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        public Vector3 position
        {
            get { return _bounds.center; }
        }

        /// <summary>
        /// Gets the attribute mask that defines the attributes for which this obstacle will not be considered an obstacle.
        /// </summary>
        /// <value>
        /// The exceptions mask.
        /// </value>
        public AttributeMask exceptionsMask
        {
            get;
            private set;
        }

        /// <summary>
        /// Toggles the obstacle on or off. This is preferred to enabling/disabling the component if it is a regularly recurring action.
        /// </summary>
        /// <param name="active">if set to <c>true</c> the obstacle is toggle on, otherwise off.</param>
        public void Toggle(bool active)
        {
            if (active == _active)
            {
                return;
            }

            var grid = GridManager.instance.GetGrid(_bounds.center);
            if (grid == null)
            {
                return;
            }

            _active = active;

            bool changed = false;
            var cells = grid.GetCoveredCells(_bounds);

            if (_active)
            {
                foreach (var c in cells)
                {
                    changed |= c.AddDynamicObstacle(this);
                }
            }
            else
            {
                foreach (var c in cells)
                {
                    changed |= c.RemoveDynamicObstacle(this);
                }
            }

            if (changed)
            {
                grid.TouchSections(_bounds);
            }
        }

        void IDynamicObstacle.ActivateUpdates(float? interval, bool repeat)
        {
            /* NOOP */
        }
    }
}
