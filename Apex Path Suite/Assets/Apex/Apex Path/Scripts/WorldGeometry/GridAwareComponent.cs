/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    using Apex.Services;
    using UnityEngine;

    /// <summary>
    /// A simple component that makes the object to which it is attached aware of the grid on which it starts out.
    /// It does NOT update the grid reference if it moves to another grid.
    /// </summary>
    [AddComponentMenu("")]
    public class GridAwareComponent : MonoBehaviour, IGridSource
    {
        /// <summary>
        /// Gets the grid.
        /// </summary>
        /// <value>
        /// The grid.
        /// </value>
        public IGrid grid
        {
            get;
            private set;
        }

        private void Start()
        {
            this.grid = GridManager.instance.GetGrid(this.transform.position);
        }
    }
}
