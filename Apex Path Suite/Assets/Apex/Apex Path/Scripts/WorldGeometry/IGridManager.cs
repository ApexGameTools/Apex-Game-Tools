/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    using System;
    using System.Collections.Generic;
    using Apex.Common;
    using Apex.DataStructures;
    using UnityEngine;

    /// <summary>
    /// Interface for grid managers.
    /// </summary>
    public interface IGridManager
    {
        /// <summary>
        /// Gets the grids managed by this manager.
        /// </summary>
        /// <value>
        /// The grids.
        /// </value>
        IEnumerable<IGrid> grids { get; }

        /// <summary>
        /// Gets the grid portals managed by this manager.
        /// </summary>
        /// <value>
        /// The portals.
        /// </value>
        IIndexable<GridPortal> portals { get; }

        /// <summary>
        /// Gets the grid at the specified position.
        /// </summary>
        /// <param name="pos">The position.</param>
        /// <returns>The matching grid or null if no match is found.</returns>
        IGrid GetGrid(Vector3 pos);

        /// <summary>
        /// Gets the grids intersected by the specified bounds.
        /// </summary>
        /// <param name="b">The bounds.</param>
        /// <returns>The matching grids.</returns>
        IEnumerable<IGrid> GetGrids(Bounds b);

        /// <summary>
        /// Gets the cells encapsulated by the bounds. This may include cells from different grids.
        /// </summary>
        /// <param name="b">The bounds.</param>
        /// <returns>An enumerable of all cells encapsulated.</returns>
        IEnumerable<IGridCell> GetCells(Bounds b);

        /// <summary>
        /// Registers the grid with this manager.
        /// </summary>
        /// <param name="grid">The grid.</param>
        void RegisterGrid(IGrid grid);

        /// <summary>
        /// Unregisters the grid with this manager.
        /// </summary>
        /// <param name="grid">The grid.</param>
        void UnregisterGrid(IGrid grid);

        /// <summary>
        /// Gets the grid component at the specified position. Note this is to get the actual component as opposed to the virtual grid matrix used for most purposes.
        /// </summary>
        /// <param name="pos">The position.</param>
        /// <returns>The matching grid component or null if no match is found.</returns>
        GridComponent GetGridComponent(Vector3 pos);

        /// <summary>
        /// Gets the grid components intersected by the specified bounds.
        /// </summary>
        /// <param name="b">The bounds.</param>
        /// <returns>The matching grid components.</returns>
        IEnumerable<GridComponent> GetGridComponents(Bounds b);

        /// <summary>
        /// Registers the grid component with this manager.
        /// </summary>
        /// <param name="grid">The grid component.</param>
        void RegisterGridComponent(GridComponent grid);

        /// <summary>
        /// Unregisters the grid component with this manager.
        /// </summary>
        /// <param name="grid">The grid component.</param>
        void UnregisterGridComponent(GridComponent grid);

        /// <summary>
        /// Determines if a portal exists between two grids.
        /// </summary>
        /// <param name="first">The first grid</param>
        /// <param name="second">The second grid.</param>
        /// <param name="requesterAttributes">The attribute mask of the requester, i.e. the entity asking to use the portal.</param>
        /// <returns><c>true</c> if at least one portal exists; otherwise <c>false</c></returns>
        bool PortalExists(IGrid first, IGrid second, AttributeMask requesterAttributes);

        /// <summary>
        /// Gets the portal.
        /// </summary>
        /// <param name="name">The name of the portal.</param>
        /// <returns>The portal or null if not found</returns>
        GridPortal GetPortal(string name);

        /// <summary>
        /// Registers a portal.
        /// </summary>
        /// <param name="name">The unique name of the portal.</param>
        /// <param name="portal">The portal.</param>
        string RegisterPortal(string name, GridPortal portal);

        /// <summary>
        /// Unregisters a portal.
        /// </summary>
        /// <param name="name">The portal name.</param>
        void UnregisterPortal(string name);

        /// <summary>
        /// Registers the portal component with this manager.
        /// </summary>
        /// <param name="portal">The portal component.</param>
        void RegisterPortalComponent(GridPortalComponent portal);

        /// <summary>
        /// Unregisters the portal component with this manager.
        /// </summary>
        /// <param name="portal">The portal component.</param>
        void UnregisterPortalComponent(GridPortalComponent portal);

        /// <summary>
        /// Gets the portal components that connects a grid. This includes both portal connecting the grid to other grids and portals forming internal connections within the grid.
        /// </summary>
        /// <param name="grid">The grid.</param>
        /// <returns>The portals</returns>
        IEnumerable<GridPortalComponent> GetAssociatedPortals(GridComponent grid);

        /// <summary>
        /// Gets the portal components that connects a grid. This includes both portal connecting the grid to other grids and portals forming internal connections within the grid.
        /// </summary>
        /// <param name="grid">The grid.</param>
        /// <returns>The portals</returns>
        IEnumerable<GridPortalComponent> GetAssociatedPortals(IGrid grid);

        /// <summary>
        /// Updates the specified region in the scene with regards to accessibility, i.e. if static obstacles are destroyed or terrain changes.
        /// </summary>
        /// <param name="extent">The extent to update.</param>
        /// <param name="maxMillisecondsUsedPerFrame">The maximum number of ms to update is allowed to use per frame until it is done.</param>
        void Update(Bounds extent, int maxMillisecondsUsedPerFrame);

        /// <summary>
        /// Updates the specified region of the scene with regards to accessibility, i.e. if static obstacles are destroyed or terrain changes.
        /// </summary>
        /// <param name="extent">The extent to update.</param>
        /// <param name="maxMillisecondsUsedPerFrame">The maximum number of ms to update is allowed to use per frame until it is done.</param>
        /// <param name="callback">An optional callback, which will be called once the update completes.</param>
        void Update(Bounds extent, int maxMillisecondsUsedPerFrame, Action callback);

        /// <summary>
        /// Updates the specified region of the scene with regards to accessibility, i.e. if static obstacles are destroyed or terrain changes.
        /// </summary>
        /// <param name="extent">The extent to update.</param>
        /// <param name="maxMillisecondsUsedPerFrame">The maximum number of ms to update is allowed to use per frame until it is done.</param>
        /// <param name="blockWhileUpdating">If <c>true</c> the cells about to be updated will be marked as blocked while the update is in progress.</param>
        /// <param name="callback">An optional callback, which will be called once the update completes.</param>
        void Update(Bounds extent, int maxMillisecondsUsedPerFrame, bool blockWhileUpdating, Action callback);
    }
}
