/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    using Apex.DataStructures;
    using Apex.Services;

    /// <summary>
    /// A sub division of a <see cref="Grid"/>, that covers part of the grid's cells. This is used for dynamically updating components on the grid when changes to the grid occur.
    /// </summary>
    public class GridSection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GridSection"/> class.
        /// </summary>
        /// <param name="rect">The rectangle defining the section.</param>
        public GridSection(RectangleXZ rect)
        {
            this.bounds = rect;
            this.lastChanged = -1f;
        }

        /// <summary>
        /// Gets the xz bounds of the section.
        /// </summary>
        /// <value>
        /// The xz bounds.
        /// </value>
        public RectangleXZ bounds
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the time this section registered changes.
        /// </summary>
        /// <value>
        /// The last changed time.
        /// </value>
        public float lastChanged
        {
            get;
            private set;
        }

        /// <summary>
        /// Touches this section, marking it as having changed.
        /// </summary>
        public void Touch()
        {
            this.lastChanged = UnityServices.time.time;
        }
    }
}
