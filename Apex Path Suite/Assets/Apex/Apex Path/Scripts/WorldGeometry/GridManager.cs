/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    using System;
    using System.Collections.Generic;
    using Apex.Common;
    using Apex.DataStructures;
    using UnityEngine;

    /// <summary>
    /// Manages all <see cref="Grid"/>s in the game world.
    /// </summary>
    public sealed class GridManager : IGridManager
    {
        /// <summary>
        /// The singleton instance
        /// </summary>
        public static readonly IGridManager instance = new GridManager();

        private List<IGrid> _grids;
        private List<GridComponent> _gridComponents;
        private DynamicArray<GridPortal> _portals;
        private DynamicArray<GridPortalComponent> _portalComponents;
        private Dictionary<string, GridPortal> _portalsLookup;

        /// <summary>
        /// Initializes a new instance of the <see cref="GridManager"/> class.
        /// </summary>
        public GridManager()
        {
            _grids = new List<IGrid>();
            _gridComponents = new List<GridComponent>();
            _portals = new DynamicArray<GridPortal>(0);
            _portalComponents = new DynamicArray<GridPortalComponent>(0);
            _portalsLookup = new Dictionary<string, GridPortal>(StringComparer.Ordinal);
        }

        /// <summary>
        /// Gets the grids managed by this manager.
        /// </summary>
        /// <value>
        /// The grids.
        /// </value>
        public IEnumerable<IGrid> grids
        {
            get { return _grids; }
        }

        /// <summary>
        /// Gets the grid portals managed by this manager.
        /// </summary>
        /// <value>
        /// The portals.
        /// </value>
        public IIndexable<GridPortal> portals
        {
            get { return _portals; }
        }

        /// <summary>
        /// Registers the grid with this manager.
        /// </summary>
        /// <param name="grid">The grid.</param>
        public void RegisterGrid(IGrid grid)
        {
            _grids.AddUnique(grid);
        }

        /// <summary>
        /// Unregisters the grid with this manager.
        /// </summary>
        /// <param name="grid">The grid.</param>
        public void UnregisterGrid(IGrid grid)
        {
            _grids.Remove(grid);
        }

        /// <summary>
        /// Gets the grid at the specified position.
        /// </summary>
        /// <param name="pos">The position.</param>
        /// <returns>
        /// The matching grid or null if no match is found.
        /// </returns>
        public IGrid GetGrid(Vector3 pos)
        {
            for (int i = 0; i < _grids.Count; i++)
            {
                var g = _grids[i];

                if (g.Contains(pos))
                {
                    return g;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the grids intersected by the specified bounds.
        /// </summary>
        /// <param name="b">The bounds.</param>
        /// <returns>The matching grids.</returns>
        public IEnumerable<IGrid> GetGrids(Bounds b)
        {
            for (int i = 0; i < _grids.Count; i++)
            {
                var g = _grids[i];

                if (g.bounds.Intersects(b))
                {
                    yield return g;
                }
            }
        }

        /// <summary>
        /// Gets the cells encapsulated by the bounds. This may include cells from different grids.
        /// </summary>
        /// <param name="b">The bounds.</param>
        /// <returns>An enumerable of all cells encapsulated.</returns>
        public IEnumerable<IGridCell> GetCells(Bounds b)
        {
            foreach (var grid in GetGrids(b))
            {
                foreach (var cell in grid.GetCoveredCells(b))
                {
                    yield return cell;
                }
            }
        }

        /// <summary>
        /// Gets the grid component at the specified position. Note this is to get the actual component as opposed to the virtual grid matrix used for most purposes.
        /// </summary>
        /// <param name="pos">The position.</param>
        /// <returns>The matching grid component or null if no match is found.</returns>
        public GridComponent GetGridComponent(Vector3 pos)
        {
            for (int i = 0; i < _gridComponents.Count; i++)
            {
                var g = _gridComponents[i];

                if (g.Contains(pos))
                {
                    return g;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the grid components intersected by the specified bounds.
        /// </summary>
        /// <param name="b">The bounds.</param>
        /// <returns>The matching grid components.</returns>
        public IEnumerable<GridComponent> GetGridComponents(Bounds b)
        {
            for (int i = 0; i < _gridComponents.Count; i++)
            {
                var g = _gridComponents[i];

                if (g.bounds.Intersects(b))
                {
                    yield return g;
                }
            }
        }

        /// <summary>
        /// Registers the grid component with this manager.
        /// </summary>
        /// <param name="grid">The grid component.</param>
        public void RegisterGridComponent(GridComponent grid)
        {
            _gridComponents.AddUnique(grid);
        }

        /// <summary>
        /// Unregisters the grid component with this manager.
        /// </summary>
        /// <param name="grid">The grid component.</param>
        public void UnregisterGridComponent(GridComponent grid)
        {
            _gridComponents.Remove(grid);
        }

        /// <summary>
        /// Updates the specified region in the scene with regards to accessibility, i.e. if static obstacles are destroyed or terrain changes.
        /// </summary>
        /// <param name="extent">The extent to update.</param>
        /// <param name="maxMillisecondsUsedPerFrame">The maximum number of ms to update is allowed to use per frame until it is done.</param>
        public void Update(Bounds extent, int maxMillisecondsUsedPerFrame)
        {
            Update(extent, maxMillisecondsUsedPerFrame, false, null);
        }

        /// <summary>
        /// Updates the specified region of the scene with regards to accessibility, i.e. if static obstacles are destroyed or terrain changes.
        /// </summary>
        /// <param name="extent">The extent to update.</param>
        /// <param name="maxMillisecondsUsedPerFrame">The maximum number of ms to update is allowed to use per frame until it is done.</param>
        /// <param name="callback">An optional callback, which will be called once the update completes.</param>
        public void Update(Bounds extent, int maxMillisecondsUsedPerFrame, Action callback)
        {
            Update(extent, maxMillisecondsUsedPerFrame, false, callback);
        }

        /// <summary>
        /// Updates the specified region of the scene with regards to accessibility, i.e. if static obstacles are destroyed or terrain changes.
        /// </summary>
        /// <param name="extent">The extent to update.</param>
        /// <param name="maxMillisecondsUsedPerFrame">The maximum number of ms to update is allowed to use per frame until it is done.</param>
        /// <param name="blockWhileUpdating">If <c>true</c> the cells about to be updated will be marked as blocked while the update is in progress.</param>
        /// <param name="callback">An optional callback, which will be called once the update completes.</param>
        public void Update(Bounds extent, int maxMillisecondsUsedPerFrame, bool blockWhileUpdating, Action callback)
        {
            for (int i = 0; i < _grids.Count; i++)
            {
                var g = _grids[i];

                if (g.bounds.Intersects(extent))
                {
                    g.Update(extent, maxMillisecondsUsedPerFrame, blockWhileUpdating, callback);
                }
            }
        }

        /// <summary>
        /// Determines if an active portal exists between two grids.
        /// </summary>
        /// <param name="first">The first grid</param>
        /// <param name="second">The second grid.</param>
        /// <param name="requesterAttributes">The attribute mask of the requester, i.e. the entity asking to use the portal.</param>
        /// <returns>
        ///   <c>true</c> if at least one portal exists; otherwise <c>false</c>
        /// </returns>
        public bool PortalExists(IGrid first, IGrid second, AttributeMask requesterAttributes)
        {
            int portalCount = _portals.count;
            for (int i = 0; i < portalCount; i++)
            {
                var p = _portals[i];

                if (p.enabled && p.IsUsableBy(requesterAttributes) && ((p.gridOne == first && p.gridTwo == second) || (p.gridOne == second && p.gridTwo == first)))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the portal.
        /// </summary>
        /// <param name="name">The name of the portal.</param>
        /// <returns>
        /// The portal or null if not found
        /// </returns>
        public GridPortal GetPortal(string name)
        {
            GridPortal p;
            _portalsLookup.TryGetValue(name, out p);

            return p;
        }

        /// <summary>
        /// Registers a portal.
        /// </summary>
        /// <param name="name">The unique name of the portal.</param>
        /// <param name="portal">The portal.</param>
        /// <returns>The actual name the portal received</returns>
        public string RegisterPortal(string name, GridPortal portal)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = string.Concat("Grid Portal ", (_portalsLookup.Count + 1));
            }

            int idx = 1;
            while (_portalsLookup.ContainsKey(name))
            {
                name = string.Concat("Grid Portal ", idx++);
            }

            _portalsLookup.Add(name, portal);
            _portals.Add(portal);

            return name;
        }

        /// <summary>
        /// Unregisters a portal.
        /// </summary>
        /// <param name="name">The portal name.</param>
        public void UnregisterPortal(string name)
        {
            _portalsLookup.Remove(name);

            _portals.Clear();
            foreach (var p in _portalsLookup.Values)
            {
                _portals.Add(p);
            }
        }

        /// <summary>
        /// Registers the portal component with this manager.
        /// </summary>
        /// <param name="portal">The portal component.</param>
        public void RegisterPortalComponent(GridPortalComponent portal)
        {
            _portalComponents.Add(portal);
        }

        /// <summary>
        /// Unregisters the portal component with this manager.
        /// </summary>
        /// <param name="portal">The portal component.</param>
        public void UnregisterPortalComponent(GridPortalComponent portal)
        {
            _portalComponents.Remove(portal);
        }

        /// <summary>
        /// Gets the portal components that connects a grid. This includes both portal connecting the grid to other grids and portals forming internal connections within the grid.
        /// </summary>
        /// <param name="grid">The grid.</param>
        /// <returns>The portals</returns>
        public IEnumerable<GridPortalComponent> GetAssociatedPortals(GridComponent grid)
        {
            var count = _portalComponents.count;
            for (int i = 0; i < count; i++)
            {
                var p = _portalComponents[i];
                if (p.Connects(grid))
                {
                    yield return _portalComponents[i];
                }
            }
        }

        /// <summary>
        /// Gets the portal components that connects a grid. This includes both portal connecting the grid to other grids and portals forming internal connections within the grid.
        /// </summary>
        /// <param name="grid">The grid.</param>
        /// <returns>The portals</returns>
        public IEnumerable<GridPortalComponent> GetAssociatedPortals(IGrid grid)
        {
            var count = _portalComponents.count;
            for (int i = 0; i < count; i++)
            {
                var p = _portalComponents[i];
                if (p.Connects(grid))
                {
                    yield return _portalComponents[i];
                }
            }
        }

        private static Vector3? CheckLineIntersection(IGrid g, Vector3 from, Vector3 to)
        {
            Vector3? intersect;

            var diagonalOneP1 = new Vector3(g.left.edge, 0.0f, g.bottom.edge);
            var diagonalOneP2 = new Vector3(g.right.edge, 0.0f, g.top.edge);

            if (Geometry.DoLinesIntersect(from, to, diagonalOneP1, diagonalOneP2, out intersect))
            {
                return intersect;
            }

            var diagonalTwoP1 = new Vector3(g.left.edge, 0.0f, g.top.edge);
            var diagonalTwoP2 = new Vector3(g.right.edge, 0.0f, g.bottom.edge);

            if (Geometry.DoLinesIntersect(from, to, diagonalTwoP1, diagonalTwoP2, out intersect))
            {
                return intersect;
            }

            return null;
        }
    }
}
