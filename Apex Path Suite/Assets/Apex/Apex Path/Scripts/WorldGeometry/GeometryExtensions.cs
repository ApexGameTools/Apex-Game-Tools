/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Apex.Common;
    using Apex.DataStructures;
    using Apex.Services;
    using Apex.Units;
    using Apex.Utilities;
    using UnityEngine;

    /// <summary>
    /// Various geometry related extensions.
    /// </summary>
    public static partial class GeometryExtensions
    {
        /// <summary>
        /// Gets the nearest walkable cell to a position.
        /// </summary>
        /// <param name="grid">The grid.</param>
        /// <param name="position">The position to initiate from.</param>
        /// <param name="inRelationTo">The position of approach.</param>
        /// <param name="requireWalkableFromPosition">Whether the cell to find must be accessible from the direction of <paramref name="position"/></param>
        /// <param name="maxCellDistance">The maximum cell distance to check.</param>
        /// <param name="unitProps">The unit properties.</param>
        /// <returns>The first walkable cell in the neighbourhood of <paramref name="position"/> that is the closest to <paramref name="inRelationTo"/>. If no such cell is found, null is returned.</returns>
        public static Cell GetNearestWalkableCell(this IGrid grid, Vector3 position, Vector3 inRelationTo, bool requireWalkableFromPosition, int maxCellDistance, IUnitProperties unitProps)
        {
            var cell = grid.GetCell(position);
            if (cell == null)
            {
                return null;
            }

            if (cell.IsWalkableWithClearance(unitProps))
            {
                return cell;
            }

            int dist = 1;
            var candidates = new List<Cell>();
            while (candidates.Count == 0 && dist <= maxCellDistance)
            {
                foreach (var c in grid.GetConcentricNeighbours(cell, dist++))
                {
                    if (requireWalkableFromPosition)
                    {
                        if (c.IsWalkableFromWithClearance(cell, unitProps))
                        {
                            candidates.Add(c);
                        }
                    }
                    else if (c.IsWalkableWithClearance(unitProps))
                    {
                        candidates.Add(c);
                    }
                }
            }

            Cell winner = null;
            float lowestDist = float.MaxValue;
            for (int i = 0; i < candidates.Count; i++)
            {
                var distSqr = (candidates[i].position - inRelationTo).sqrMagnitude;
                if (distSqr < lowestDist)
                {
                    winner = candidates[i];
                    lowestDist = distSqr;
                }
            }

            return winner;
        }

        /// <summary>
        /// Gets the nearest walkable cell to a position seen in relation to another position.
        /// </summary>
        /// <param name="grid">The grid.</param>
        /// <param name="position">The position to check.</param>
        /// <param name="inRelationTo">The relative position.</param>
        /// <param name="requireWalkableFromPosition">Whether the cell to find must be accessible from the direction of <paramref name="position"/></param>
        /// <param name="maxCellDistance">The maximum cell distance to check.</param>
        /// <param name="unitProps">The unit properties.</param>
        /// <returns></returns>
        public static Cell GetNearestWalkableCellFromRelation(this IGrid grid, Vector3 position, Vector3 inRelationTo, bool requireWalkableFromPosition, int maxCellDistance, IUnitProperties unitProps)
        {
            var cell = grid.GetCell(position);
            if (cell == null)
            {
                return null;
            }

            if (cell.IsWalkableWithClearance(unitProps))
            {
                return cell;
            }

            var relationCell = grid.GetCell(inRelationTo);
            if (relationCell == null)
            {
                return null;
            }

            int dist = 1;
            var candidates = new List<Cell>();
            while (candidates.Count == 0 && dist <= maxCellDistance)
            {
                foreach (var c in grid.GetConcentricNeighbours(cell, dist++))
                {
                    if (requireWalkableFromPosition)
                    {
                        if (c.IsWalkableFromWithClearance(relationCell, unitProps))
                        {
                            candidates.Add(c);
                        }
                    }
                    else if (c.IsWalkableWithClearance(unitProps))
                    {
                        candidates.Add(c);
                    }
                }
            }

            Cell winner = null;
            float lowestDist = float.MaxValue;
            for (int i = 0; i < candidates.Count; i++)
            {
                var distSqr = (candidates[i].position - inRelationTo).sqrMagnitude;
                if (distSqr < lowestDist)
                {
                    winner = candidates[i];
                    lowestDist = distSqr;
                }
            }

            return winner;
        }

        /// <summary>
        /// Gets the walkable neighbours.
        /// </summary>
        /// <param name="grid">The grid</param>
        /// <param name="c">The cell whose walkable neighbours to return.</param>
        /// <param name="unitProps">The unit properties</param>
        /// <param name="excludeCornerCutting">if set to <c>true</c> otherwise walkable neighbours on the diagonal that would cause a move from it to the current cell to cut a corner are excluded (deemed not walkable).</param>
        /// <returns>The walkable neighbours to the referenced cell.</returns>
        public static IEnumerable<Cell> GetWalkableNeighbours(this IGrid grid, IGridCell c, IUnitProperties unitProps, bool excludeCornerCutting)
        {
            Cell n;

            //Straight move neighbours
            bool uw = grid.TryGetWalkableNeighbour(c, 0, 1, unitProps, out n);
            if (uw)
            {
                yield return n;
            }

            bool dw = grid.TryGetWalkableNeighbour(c, 0, -1, unitProps, out n);
            if (dw)
            {
                yield return n;
            }

            bool rw = grid.TryGetWalkableNeighbour(c, 1, 0, unitProps, out n);
            if (rw)
            {
                yield return n;
            }

            bool lw = grid.TryGetWalkableNeighbour(c, -1, 0, unitProps, out n);
            if (lw)
            {
                yield return n;
            }

            //Diagonal neighbours. First determine if they are unwalkable as a consequence of their straight neighbours
            bool urw, drw, dlw, ulw;
            if (excludeCornerCutting)
            {
                urw = uw && rw;
                drw = dw && rw;
                dlw = dw && lw;
                ulw = uw && lw;
            }
            else
            {
                urw = uw || rw;
                drw = dw || rw;
                dlw = dw || lw;
                ulw = uw || lw;
            }

            urw = urw && grid.TryGetWalkableNeighbour(c, 1, 1, unitProps, out n);
            if (urw)
            {
                yield return n;
            }

            drw = drw && grid.TryGetWalkableNeighbour(c, 1, -1, unitProps, out n);
            if (drw)
            {
                yield return n;
            }

            dlw = dlw && grid.TryGetWalkableNeighbour(c, -1, -1, unitProps, out n);
            if (dlw)
            {
                yield return n;
            }

            ulw = ulw && grid.TryGetWalkableNeighbour(c, -1, 1, unitProps, out n);
            if (ulw)
            {
                yield return n;
            }
        }

        /// <summary>
        /// Determines whether a cell is walkable to all (regardless of their attributes).
        /// </summary>
        /// <param name="cell">The cell.</param>
        /// <returns><c>true</c> if walkable to all units, otherwise <c>false</c></returns>
        public static bool IsWalkableToAll(this IGridCell cell)
        {
            return cell.IsWalkable(AttributeMask.None);
        }

        /// <summary>
        /// Determines whether is walkable to any unit, i.e. if at least one of the defined special attributes will make the cell resolve as walkable.
        /// </summary>
        /// <param name="cell">The cell.</param>
        /// <returns><c>true</c> if walkable to any units, otherwise <c>false</c></returns>
        public static bool IsWalkableToAny(this IGridCell cell)
        {
            return cell.IsWalkable(AttributeMask.All);
        }

        /// <summary>
        /// Samples the height at the specified position.
        /// </summary>
        /// <param name="grid">The grid</param>
        /// <param name="position">The position.</param>
        /// <returns>The height at the position</returns>
        public static float SampleHeight(this IGrid grid, Vector3 position)
        {
            Ensure.ArgumentNotNull(grid, "grid");

            return GameServices.heightStrategy.heightSampler.SampleHeight(position, grid.cellMatrix);
        }

        /// <summary>
        /// Tries to sample the height at the specified position.
        /// </summary>
        /// <param name="grid">The gid</param>
        /// <param name="position">The position.</param>
        /// <param name="height">The height at the position.</param>
        /// <returns><c>true</c> if the position represents valid terrain and a height could be found; otherwise <c>false</c></returns>
        public static bool TrySampleHeight(this IGrid grid, Vector3 position, out float height)
        {
            Ensure.ArgumentNotNull(grid, "grid");

            return GameServices.heightStrategy.heightSampler.TrySampleHeight(position, grid.cellMatrix, out height);
        }

        /// <summary>
        /// Updates the specified region of the scene with regards to accessibility, i.e. if static obstacles are destroyed or terrain changes.
        /// </summary>
        /// <param name="extent">The extent to update.</param>
        /// <param name="maxMillisecondsUsedPerFrame">The maximum number of ms to update is allowed to use per frame until it is done.</param>
        /// <param name="blockWhileUpdating">If <c>true</c> the cells about to be updated will be marked as blocked while the update is in progress.</param>
        /// <param name="callback">An optional callback, which will be called once the update completes.</param>
        public static void Update(this Bounds extent, int maxMillisecondsUsedPerFrame, bool blockWhileUpdating, Action callback)
        {
            GridManager.instance.Update(extent, maxMillisecondsUsedPerFrame, blockWhileUpdating, callback);
        }

        /// <summary>
        /// Gets the neighbour grid component in the given direction, if one exists.
        /// The predefined directions on the <see cref="DirectionVector"/> can be combined for more combinations.
        /// </summary>
        /// <param name="grid">The grid.</param>
        /// <param name="direction">The direction.</param>
        /// <returns>The neighbouring grid, or null if no neighbour exists</returns>
        public static GridComponent GetNeighbourGrid(this IGrid grid, DirectionVector direction)
        {
            var bounds = grid.bounds;
            var pos = bounds.center + ((bounds.extents + Vector3.one) * direction);

            return GridManager.instance.GetGridComponent(pos);
        }

        /// <summary>
        /// Gets the neighbour grid component in the given direction, if one exists.
        /// The predefined directions on the <see cref="DirectionVector"/> can be combined for more combinations.
        /// </summary>
        /// <param name="grid">The grid.</param>
        /// <param name="direction">The direction.</param>
        /// <returns>The neighbouring grid, or null if no neighbour exists</returns>
        public static GridComponent GetNeighbourGrid(this GridComponent grid, DirectionVector direction)
        {
            var bounds = grid.bounds;
            var pos = bounds.center + ((bounds.extents + Vector3.one) * direction);

            return GridManager.instance.GetGridComponent(pos);
        }

        /// <summary>
        /// Gets the neighbour grid component to a given position in the given direction, if one exists.
        /// The predefined directions on the <see cref="DirectionVector"/> can be combined for more combinations.
        /// </summary>
        /// <param name="mgr">The grid manager.</param>
        /// <param name="position">The position.</param>
        /// <param name="direction">The direction.</param>
        /// <returns>The neighbouring grid, or null if no neighbour exists</returns>
        public static GridComponent GetNeighbourGrid(this IGridManager mgr, Vector3 position, DirectionVector direction)
        {
            var g = mgr.GetGridComponent(position);

            var bounds = g.bounds;
            var pos = bounds.center + ((bounds.extents + Vector3.one) * direction);

            return mgr.GetGridComponent(pos);
        }

        /// <summary>
        /// Connects the specified grid to its neighbour grid(s) using Connector Portals. Will only connect to initialized and enabled grids.
        /// </summary>
        /// <param name="grid">The grid.</param>
        /// <param name="pos">The position to connect with neighbours. Valid options are Top, Bottom, Left and Right</param>
        /// <returns>A list of portal components that represent the connections. If none are found the list will be empty.</returns>
        /// <exception cref="System.ArgumentException">
        /// Can only connect a grid component that has been initialized.
        /// or
        /// Only direct neighbours, i.e. Left, Right, Top or Bottom are supported.
        /// </exception>
        public static IList<GridPortalComponent> Connect(this GridComponent grid, NeighbourPosition pos)
        {
            if (grid.grid == null)
            {
                throw new ArgumentException("Can only connect a grid component that has been initialized.");
            }

            NeighbourPosition neighboursPos;
            Vector3 translation;
            Bounds explorerBounds;
            float connectorPortalWidth = grid.connectorPortalWidth;

            //First we establish an origin based bounds on the appropriate side of the grid to check for neighbour grids
            switch (pos)
            {
                case NeighbourPosition.Bottom:
                {
                    neighboursPos = NeighbourPosition.Top;
                    translation = new Vector3(0f, 0f, connectorPortalWidth);
                    explorerBounds = GetEdge(grid.grid, NeighbourPosition.Bottom, connectorPortalWidth).Translate(-translation);
                    break;
                }

                case NeighbourPosition.Top:
                {
                    neighboursPos = NeighbourPosition.Bottom;
                    translation = new Vector3(0f, 0f, -connectorPortalWidth);
                    explorerBounds = GetEdge(grid.grid, NeighbourPosition.Top, connectorPortalWidth).Translate(-translation);
                    break;
                }

                case NeighbourPosition.Left:
                {
                    neighboursPos = NeighbourPosition.Right;
                    translation = new Vector3(connectorPortalWidth, 0f, 0f);
                    explorerBounds = GetEdge(grid.grid, NeighbourPosition.Left, connectorPortalWidth).Translate(-translation);
                    break;
                }

                case NeighbourPosition.Right:
                {
                    neighboursPos = NeighbourPosition.Left;
                    translation = new Vector3(-connectorPortalWidth, 0f, 0f);
                    explorerBounds = GetEdge(grid.grid, NeighbourPosition.Right, connectorPortalWidth).Translate(-translation);
                    break;
                }

                default:
                {
                    throw new ArgumentException("Only direct neighbours, i.e. Left, Right, Top or Bottom are supported.");
                }
            }

            //Resize so the bounds are contained in the grid
            explorerBounds = explorerBounds.DeltaSize(-0.05f, 0f, -0.05f);

            var existingConnectors = GridManager.instance.GetAssociatedPortals(grid).Where(p => p.type == PortalType.Connector).ToArray();

            var portals = new List<GridPortalComponent>();
            var neighbours = GridManager.instance.GetGrids(explorerBounds);
            foreach (var n in neighbours)
            {
                //Make sure a connection is not already in place
                bool connect = true;
                for (int i = 0; i < existingConnectors.Length; i++)
                {
                    if (existingConnectors[i].Connects(grid.grid, n))
                    {
                        connect = false;
                        break;
                    }
                }

                if (connect)
                {
                    //Get the edge on the neighbour grid and find the intersection between it and this grid
                    var p1 = GetEdge(n, neighboursPos, grid.connectorPortalWidth).DeltaSize(-0.05f, 0f, -0.05f);
                    p1 = p1.Intersection(explorerBounds);
                    var p2 = p1.Translate(translation);

                    var portal = GridPortalComponent.CreateConnector(grid.gameObject, p1, p2);
                    portals.Add(portal);
                }
            }

            return portals;
        }

        /// <summary>
        /// Gets a edge bounds, i.e. a side of a grid <paramref name="width"/> wide.
        /// </summary>
        /// <param name="grid">The grid.</param>
        /// <param name="pos">The position.</param>
        /// <param name="width">The width if the bounds.</param>
        /// <returns>The bounds representing a side of the grid.</returns>
        /// <exception cref="System.ArgumentException">Only direct neighbours, i.e. Left, Right, Top or Bottom are supported.</exception>
        public static Bounds GetEdge(this IGrid grid, NeighbourPosition pos, float width)
        {
            Bounds edgeBounds = new Bounds();

            var min = grid.bounds.min;
            var max = grid.bounds.max;
            min.y = grid.origin.y;
            max.y = Mathf.Min(0.1f, max.y);

            //First we establish an origin based bounds on the appropriate side of the grid
            switch (pos)
            {
                case NeighbourPosition.Bottom:
                {
                    max.z = min.z + width;
                    edgeBounds.SetMinMax(min, max);

                    return edgeBounds;
                }

                case NeighbourPosition.Top:
                {
                    min.z = max.z - width;
                    edgeBounds.SetMinMax(min, max);

                    return edgeBounds;
                }

                case NeighbourPosition.Left:
                {
                    max.x = min.x + width;
                    edgeBounds.SetMinMax(min, max);

                    return edgeBounds;
                }

                case NeighbourPosition.Right:
                {
                    min.x = max.x - width;
                    edgeBounds.SetMinMax(min, max);

                    return edgeBounds;
                }

                default:
                {
                    throw new ArgumentException("Only direct neighbours, i.e. Left, Right, Top or Bottom are supported.");
                }
            }
        }
    }
}
