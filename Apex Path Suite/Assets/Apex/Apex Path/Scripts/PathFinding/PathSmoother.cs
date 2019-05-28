/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.PathFinding
{
    using System;
    using System.Collections.Generic;
    using Apex.Units;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// The default path smoothing implementation. This uses tangents between points to optimize routes.
    /// </summary>
    public class PathSmoother : ISmoothPaths
    {
        /// <summary>
        /// Determines whether an unobstructed path exists between two points.
        /// </summary>
        /// <param name="from">The from point.</param>
        /// <param name="to">The to point.</param>
        /// <param name="unitProps">The unit properties to test against (Walkability).</param>
        /// <param name="matrix">The matrix where both <paramref name="from"/> and <paramref name="to"/> must be part of.</param>
        /// <param name="costStrategy">The cost strategy.</param>
        /// <returns><c>true</c> if an unobstructed path exists between the two points; otherwise <c>false</c>.</returns>
        public static bool CanReducePath(IPositioned from, IPositioned to, IUnitProperties unitProps, CellMatrix matrix, ICellCostStrategy costStrategy)
        {
            //The reference cell is always the from cell. The fact the points are swapped to simplify tangents does not affect this.
            var refCell = matrix.GetCell(from.position, false);

            Vector3 p1;
            Vector3 p2;

            //Assign the points so we start with the point with the lowest x-value to simplify things
            if (from.position.x > to.position.x)
            {
                p1 = to.position;
                p2 = from.position;
            }
            else
            {
                p1 = from.position;
                p2 = to.position;
            }

            var unitRadius = unitProps.radius;
            var tan = Tangents.Create(p1, p2, unitRadius, matrix);

            var slopeDir = tan.slopeDir;
            var cellSize = matrix.cellSize;
            var halfCell = cellSize * 0.5f;

            //Get the start and end cells, get the cost of the actual start and end, and then reassign the start and end to accommodate for unit size.
            var startCell = matrix.GetCell(p1, false);
            var startCost = costStrategy.GetCellCost(startCell, unitProps);
            startCell = matrix.GetCell(new Vector3(p1.x - unitRadius, p1.y, p1.z), true);

            var endCell = matrix.GetCell(p2, false);
            var endCost = costStrategy.GetCellCost(endCell, unitProps);
            endCell = matrix.GetCell(new Vector3(p2.x + unitRadius, p2.y, p2.z), true);

            //The movement across the x-axis
            float minXCoord = (startCell.position.x - matrix.start.x) - halfCell;
            float maxXCoord = (startCell.position.x - matrix.start.x) + halfCell;

            int minZ;
            int maxZ;
            if (slopeDir < 0)
            {
                var distLowerCellBoundary = halfCell - (endCell.position.z - p2.z);
                var minOverlap = Mathf.CeilToInt((unitRadius - distLowerCellBoundary) / cellSize);
                minZ = Math.Max(0, endCell.matrixPosZ - Math.Max(0, minOverlap));

                var distUpperCellBoundary = halfCell - (p1.z - startCell.position.z);
                var maxOverlap = Mathf.CeilToInt((unitRadius - distUpperCellBoundary) / cellSize);
                maxZ = Math.Min(matrix.rows - 1, startCell.matrixPosZ + Math.Max(0, maxOverlap));
            }
            else
            {
                var distLowerCellBoundary = halfCell - (startCell.position.z - p1.z);
                var minOverlap = Mathf.CeilToInt((unitRadius - distLowerCellBoundary) / cellSize);
                minZ = Math.Max(0, startCell.matrixPosZ - Math.Max(0, minOverlap));

                var distUpperCellBoundary = halfCell - (p2.z - endCell.position.z);
                var maxOverlap = Mathf.CeilToInt((unitRadius - distUpperCellBoundary) / cellSize);
                maxZ = Math.Min(matrix.rows - 1, endCell.matrixPosZ + Math.Max(0, maxOverlap));
            }

            int startX = startCell.matrixPosX;
            int endX = endCell.matrixPosX;
            bool isVertical = tan.isVertical;
            var cellMatrix = matrix.rawMatrix;
            for (int x = startX; x <= endX; x++)
            {
                int startZ;
                int endZ;

                if (isVertical)
                {
                    startZ = Math.Min(startCell.matrixPosZ, endCell.matrixPosZ);
                    endZ = Math.Max(startCell.matrixPosZ, endCell.matrixPosZ);
                }
                else
                {
                    if (slopeDir < 0)
                    {
                        startZ = Math.Max((int)(tan.LowTangent(maxXCoord) / cellSize), minZ);
                        endZ = Math.Min((int)(tan.HighTangent(minXCoord) / cellSize), maxZ);
                    }
                    else
                    {
                        startZ = Math.Max((int)(tan.LowTangent(minXCoord) / cellSize), minZ);
                        endZ = Math.Min((int)(tan.HighTangent(maxXCoord) / cellSize), maxZ);
                    }
                }

                for (int z = startZ; z <= endZ; z++)
                {
                    var intermediary = cellMatrix[x, z];
                    if (!isVertical && tan.IsOutsideSecants(intermediary.position))
                    {
                        continue;
                    }

                    var intermediaryCost = costStrategy.GetCellCost(intermediary, unitProps);
                    if (!intermediary.IsWalkableFrom(refCell, unitProps) || (startCost < intermediaryCost || endCost < intermediaryCost))
                    {
                        return false;
                    }
                }

                minXCoord = maxXCoord;
                maxXCoord = maxXCoord + cellSize;
            }

            return true;
        }

        /// <summary>
        /// Smooths a path.
        /// </summary>
        /// <param name="goal">The goal node of the calculated path.</param>
        /// <param name="maxPathLength">Maximum length of the path.</param>
        /// <param name="request">The path request.</param>
        /// <param name="costStrategy">The cell cost provider.</param>
        /// <returns>
        /// The path in smoothed form
        /// </returns>
        public Path Smooth(IPathNode goal, int maxPathLength, IPathRequest request, ICellCostStrategy costStrategy)
        {
            var unitProps = request.requesterProperties;

            //Next prune superfluous path nodes
            var reversePath = new List<IPositioned>(maxPathLength);

            var current = goal;
            var next = current.predecessor;

            int bends = -1;
            var prevDir = Vector3.zero;

            while (next != null)
            {
                var dir = next.position - current.position;

                if ((dir != prevDir) || (next is IPortalNode))
                {
                    reversePath.Add(current);
                    prevDir = dir;
                    bends++;
                }

                current = next;
                next = current.predecessor;
            }

            //Correct the end nodes and inject a mid point if too much was pruned (can happen on straight paths with no direction change, which can lead to obstacle collision if the unit is offset)
            if (reversePath.Count == 0)
            {
                reversePath.Add(new Position(request.to));
            }
            else
            {
                reversePath[0] = new Position(request.to);
            }

            if (reversePath.Count == 1 && bends <= 0)
            {
                reversePath.Add(goal.predecessor);
            }

            reversePath.Add(new Position(request.from));
            int pathLength = reversePath.Count;

            //Next see if we can reduce the path further by excluding unnecessary bends
            var startGrid = GridManager.instance.GetGrid(request.from);
            if (!request.pathFinderOptions.preventDiagonalMoves && startGrid != null)
            {
                var matrix = startGrid.cellMatrix;

                int indexLimit = 2;
                for (int i = reversePath.Count - 1; i >= indexLimit; i--)
                {
                    var c1 = reversePath[i];
                    var c2 = reversePath[i - 1];
                    var c3 = reversePath[i - 2];

                    var skip = AdjustIfPortal(c1, c2, c3);

                    if (skip > -1)
                    {
                        //One of the candidate nodes is a portal so skip to the node following the portal and resolve the grid at the other end of the portal.
                        //Since a portal node will never be the last node we can safely do this here. Since we are moving in the reverse direction here the portal will be on the other side.
                        i -= skip;
                        matrix = ((IPortalNode)reversePath[i]).partner.parent;
                        continue;
                    }

                    while (CanReducePath(c1, c3, unitProps, matrix, costStrategy))
                    {
                        reversePath[i - 1] = null;
                        pathLength--;
                        i--;

                        if (i < indexLimit)
                        {
                            break;
                        }

                        c3 = reversePath[i - 2];
                        if (c3 is IPortalNode)
                        {
                            break;
                        }
                    }
                }
            }

            //Construct the final path
            var path = new Path(pathLength);

            var count = reversePath.Count;
            for (int i = 0; i < count; i++)
            {
                var node = reversePath[i];
                if (node != null)
                {
                    path.Push(node);
                }
            }

            return path;
        }

        private static int AdjustIfPortal(params IPositioned[] nodes)
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i] is IPortalNode)
                {
                    return i;
                }
            }

            return -1;
        }

        private struct Tangents
        {
            public int slopeDir;

            private float _alpha;
            private float _alphaSec;
            private float _betaTanHigh;
            private float _betaTanLow;
            private float _betaSecLow;
            private float _betaSecHigh;

            public bool isVertical
            {
                get;
                private set;
            }

            public static Tangents Create(Vector3 from, Vector3 to, float circleRadius, CellMatrix matrix)
            {
                var t = new Tangents();
                t.Init(from, to, circleRadius, matrix);

                return t;
            }

            public float HighTangent(float x)
            {
                return (_alpha * x) + _betaTanHigh;
            }

            public float LowTangent(float x)
            {
                return (_alpha * x) + _betaTanLow;
            }

            public bool IsOutsideSecants(Vector3 p)
            {
                return ((_alphaSec * p.x) + _betaSecLow > p.z) || ((_alphaSec * p.x) + _betaSecHigh < p.z);
            }

            private void Init(Vector3 from, Vector3 to, float circleRadius, CellMatrix matrix)
            {
                var dx = to.x - from.x;
                var dz = to.z - from.z;

                slopeDir = Math.Sign(dz);
                if (slopeDir == 0)
                {
                    slopeDir = 1;
                }

                //Due to floating point errors, we have to treat low x deltas as vertical
                if (Mathf.Abs(dx) < 0.00001f)
                {
                    this.isVertical = true;
                    return;
                }

                _alpha = dz / dx;

                //instead of handling horizontals specifically (and hence vertical secants), we just treat it as a very small alpha
                _alphaSec = (dz == 0f) ? -1000000f : -dx / dz;
                var ortho = new Vector3(-dz, 0f, dx).normalized * circleRadius;

                if (slopeDir < 0)
                {
                    _betaSecLow = to.z - (_alphaSec * to.x);
                    _betaSecHigh = from.z - (_alphaSec * from.x);
                }
                else
                {
                    _betaSecLow = from.z - (_alphaSec * from.x);
                    _betaSecHigh = to.z - (_alphaSec * to.x);
                }

                from = from - matrix.start;

                var tmp = from + ortho;
                _betaTanHigh = tmp.z - (_alpha * tmp.x);

                tmp = from - ortho;
                _betaTanLow = tmp.z - (_alpha * tmp.x);
            }
        }
    }
}
