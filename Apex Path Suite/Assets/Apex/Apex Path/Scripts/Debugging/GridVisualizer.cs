/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Debugging
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Apex;
    using Apex.Common;
    using Apex.DataStructures;
    using Apex.Steering;
    using Apex.Units;
    using Apex.Utilities;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// Visualization component that draws gizmos to represent the grid and show obstructed areas.
    /// </summary>
    [ExecuteInEditMode]
    [AddComponentMenu("Apex/Game World/Debugging/Grid Visualizer", 1013)]
    [ApexComponent("Debugging")]
    public class GridVisualizer : Visualizer
    {
        /// <summary>
        /// Controls how the grid is drawn
        /// </summary>
        [Tooltip("Controls how the grid visualization is drawn.")]
        public GridMode drawMode;

        /// <summary>
        /// The height navigation capabilities used to model accessibility
        /// </summary>
        public HeightNavigationCapabilities modelHeightNavigationCapabilities = new HeightNavigationCapabilities
        {
            maxClimbHeight = 0.5f,
            maxDropHeight = 1f,
            maxSlopeAngle = 30f
        };

        /// <summary>
        /// The model attributes
        /// </summary>
        [AttributeProperty("Model Attributes", "The attributes of the model unit for which to show the visualization")]
        public int modelAttributes;

        /// <summary>
        /// Whether to draw the grids sub sections
        /// </summary>
        [Tooltip("Draw a crude representation of the grid's sub sections.")]
        public bool drawSubSections;

        /// <summary>
        /// Controls whether the visualizer draws all grids in the scene or only those on the same GameObject as the visualizer.
        /// </summary>
        [Tooltip("Draw all grids in the scene, i.e. only one visualizer needed.")]
        public bool drawAllGrids = false;

        /// <summary>
        /// The editor refresh delay, i.e. increase this value if you experience too much CPU activity during scene modification
        /// </summary>
        [Tooltip("The delay between updates after changes have been made to the scene.")]
        public int editorRefreshDelay = 100;

        /// <summary>
        /// The distance threshold controlling when the grid is drawn. If the camera is showing more than this looking at the diagonal, the grid will no longer be drawn.
        /// This is for performance reasons plus when zoomed out too far the grid visualization is useless.
        /// </summary>
        [Tooltip("How zoomed out can you be and still see the grid visuals.")]
        public float drawDistanceThreshold = 175f;

        /// <summary>
        /// The grid lines color
        /// </summary>
        [Tooltip("The color of the grid lines.")]
        public Color gridLinesColor = new Color(135f / 255f, 135f / 255f, 135f / 255f);

        /// <summary>
        /// The color of accessibility lines where only descent is possible
        /// </summary>
        [Tooltip("The color of accessibility lines where only descent is possible.")]
        public Color descentOnlyColor = new Color(175f / 255f, 6175 / 255f, 60 / 255f);

        /// <summary>
        /// The color of accessibility lines where only ascent is possible
        /// </summary>
        [Tooltip("The color of accessibility lines where only ascent is possible.")]
        public Color ascentOnlyColor = new Color(60 / 255f, 60f / 255f, 175f / 255f);

        /// <summary>
        /// The obstacle color
        /// </summary>
        [Tooltip("The color of obstructed cells.")]
        public Color obstacleColor = new Color(226f / 255f, 41f / 255f, 32f / 255f, 150f / 255f);

        /// <summary>
        /// The sub sections color
        /// </summary>
        [Tooltip("The color of the sub sections visualization.")]
        public Color subSectionsColor = new Color(0f, 150f / 255f, 211f / 255f);

        /// <summary>
        /// The color of the bounds wire frame
        /// </summary>
        [Tooltip("The color of the bounds frame.")]
        public Color boundsColor = Color.grey;

        private DateTime? _nextRefresh;
        private ModelProps _modelProps = new ModelProps();

        /// <summary>
        /// How the grid visualization is displayed
        /// </summary>
        public enum GridMode
        {
            /// <summary>
            /// Draws the grid to represent how it is actually laid out
            /// </summary>
            Layout,

            /// <summary>
            /// Draws the grid by showing accessibility between grid cells
            /// </summary>
            Accessibility,

            /// <summary>
            /// Draws the height overlay representing the grids height map
            /// </summary>
            HeightOverlay
        }

        /// <summary>
        /// Draws the actual visualization.
        /// </summary>
        protected override void DrawVisualization()
        {
            _modelProps.heightNavigationCapability = this.modelHeightNavigationCapabilities;
            _modelProps.attributes = this.modelAttributes;

            var forceRefresh = false;
            if (_nextRefresh.HasValue && _nextRefresh < DateTime.UtcNow)
            {
                _nextRefresh = null;
                forceRefresh = true;
            }

            var grids = this.drawAllGrids ? FindObjectsOfType<GridComponent>() : GetComponents<GridComponent>();

            if (grids != null)
            {
                foreach (var grid in grids)
                {
                    if (grid.enabled)
                    {
                        bool outlineOnly;
                        var drawArea = GetDrawArea(grid.origin.y, out outlineOnly);
                        if (!drawArea.Overlaps(grid.bounds))
                        {
                            continue;
                        }

                        drawArea = grid.EnsureForEditor(forceRefresh, drawArea);
                        if (!DrawGrid(grid, drawArea, outlineOnly))
                        {
                            grid.EnsureForEditor(true, drawArea);
                            DrawGrid(grid, drawArea, outlineOnly);
                        }
                    }
                }
            }
        }

        private Bounds GetDrawArea(float gridElevation, out bool outlineOnly)
        {
            outlineOnly = false;

            //Determine the area visible through the camera, starting with the bottom left and top right corners.
            var cam = Camera.current;
            var bl = cam.ScreenToGroundPoint(Vector3.zero, gridElevation);
            var tr = cam.ScreenToGroundPoint(new Vector3(cam.pixelWidth, cam.pixelHeight, 0f), gridElevation);
            if (bl == Vector3.zero || tr == Vector3.zero)
            {
                outlineOnly = true;
            }

            //We do not want to draw the grids if zoom level is too high since it will simply crash the editor to draw that many lines.
            var diagDistSquared = (tr - bl).sqrMagnitude;
            if (diagDistSquared > this.drawDistanceThreshold * this.drawDistanceThreshold)
            {
                outlineOnly = true;
            }

            //Get the remaining corners resolved and calculate the proper bounds to draw
            var tl = cam.ScreenToGroundPoint(new Vector3(0f, cam.pixelHeight, 0f), gridElevation);
            var br = cam.ScreenToGroundPoint(new Vector3(cam.pixelWidth, 0f, 0f), gridElevation);

            var wpMin = new Vector3(Mathf.Min(bl.x, tl.x, br.x, tr.x), 0f, Mathf.Min(bl.z, tl.z, br.z, tr.z));
            var wpMax = new Vector3(Mathf.Max(bl.x, tl.x, br.x, tr.x), 0f, Mathf.Max(bl.z, tl.z, br.z, tr.z));
            var drawArea = new Bounds();
            drawArea.SetMinMax(wpMin, wpMax);

            return drawArea;
        }

        private bool DrawGrid(GridComponent gridComp, Bounds drawArea, bool outlineOnly)
        {
            if (gridComp.sizeX == 0 || gridComp.sizeZ == 0 || gridComp.cellSize == 0f)
            {
                return true;
            }

            Gizmos.color = this.boundsColor;
            Gizmos.DrawWireCube(gridComp.bounds.center, gridComp.bounds.size);

            IGrid grid = gridComp.grid;
            if (outlineOnly || grid == null)
            {
                return true;
            }

            var start = grid.GetCell(drawArea.min);
            var end = grid.GetCell(drawArea.max);
            if (start == null || end == null)
            {
                return false;
            }

            var lineColor = this.gridLinesColor;
            if (!gridComp.automaticInitialization && !Application.isPlaying)
            {
                lineColor.a = lineColor.a * 0.5f;
            }

            if (this.drawMode == GridMode.Layout)
            {
                DrawLayout(grid, start, end, lineColor);
            }
            else if (this.drawMode == GridMode.HeightOverlay)
            {
                grid.cellMatrix.RenderHeightOverlay(this.gridLinesColor);
            }
            else
            {
                DrawAccessibility(grid, start, end, lineColor);
            }

            if (this.drawSubSections)
            {
                var y = grid.origin.y + 0.05f;

                Gizmos.color = this.subSectionsColor;
                foreach (var section in grid.gridSections)
                {
                    var subCenter = section.bounds.center;
                    subCenter.y = y;
                    Gizmos.DrawWireCube(subCenter, section.bounds.size);
                }
            }

            return true;
        }

        private bool DrawLayout(IGrid grid, Cell start, Cell end, Color lineColor)
        {
            Gizmos.color = lineColor;

            var step = grid.cellSize;
            var halfCell = (step / 2.0f);
            var y = grid.origin.y + 0.05f;

            var xMin = start.position.x - halfCell;
            var xMax = end.position.x + halfCell;
            var zMin = start.position.z - halfCell;
            var zMax = end.position.z + halfCell;

            for (float x = xMin; x <= xMax; x += step)
            {
                Gizmos.DrawLine(new Vector3(x, y, zMin), new Vector3(x, y, zMax));
            }

            for (float z = zMin; z <= zMax; z += step)
            {
                Gizmos.DrawLine(new Vector3(xMin, y, z), new Vector3(xMax, y, z));
            }

            var matrix = grid.cellMatrix;
            for (int x = start.matrixPosX; x <= end.matrixPosX; x++)
            {
                for (int z = start.matrixPosZ; z <= end.matrixPosZ; z++)
                {
                    var c = matrix[x, z];
                    if (c == null)
                    {
                        return false;
                    }

                    var walkableToSomeone = c.IsWalkable(AttributeMask.All);
                    var walkableToEveryone = c.IsWalkable(AttributeMask.None);

                    if (!walkableToSomeone)
                    {
                        Gizmos.color = this.obstacleColor;
                        Gizmos.DrawCube(c.position, new Vector3(step, 0.05f, step));
                    }
                    else if (!walkableToEveryone)
                    {
                        var half = this.obstacleColor;
                        half.a = half.a * 0.5f;
                        Gizmos.color = half;
                        Gizmos.DrawCube(c.position, new Vector3(step, 0.05f, step));
                    }
                }
            }

            return true;
        }

        private bool DrawAccessibility(IGrid grid, Cell start, Cell end, Color lineColor)
        {
            var matrix = grid.cellMatrix;
            var cellSize = grid.cellSize;

            VectorXZ[] directions = new[] { new VectorXZ(-1, 0), new VectorXZ(-1, 1), new VectorXZ(0, 1), new VectorXZ(1, 1) };
            var heightAdj = new Vector3(0.0f, 0.05f, 0.0f);

            for (int x = start.matrixPosX; x <= end.matrixPosX; x++)
            {
                for (int z = start.matrixPosZ; z <= end.matrixPosZ; z++)
                {
                    var c = matrix[x, z];
                    if (c == null)
                    {
                        return false;
                    }

                    if (!c.IsWalkableWithClearance(_modelProps))
                    {
                        Gizmos.color = this.obstacleColor;
                        Gizmos.DrawCube(c.position, new Vector3(cellSize, 0.05f, cellSize));
                        continue;
                    }

                    var curPos = new VectorXZ(x, z);
                    for (int i = 0; i < 4; i++)
                    {
                        var checkPos = curPos + directions[i];
                        var other = matrix[checkPos.x, checkPos.z];

                        if (other != null)
                        {
                            if (!other.IsWalkableWithClearance(_modelProps))
                            {
                                continue;
                            }

                            //Determine top and bottom, with bottom winning over top if equal so to speak, due to cross height.
                            var topPos = c;
                            var bottomPos = other;
                            if (topPos.position.y <= bottomPos.position.y)
                            {
                                topPos = bottomPos;
                                bottomPos = c;
                            }

                            var topDown = bottomPos.IsWalkableFromWithClearance(topPos, _modelProps);
                            var downTop = topPos.IsWalkableFromWithClearance(bottomPos, _modelProps);

                            if (topDown && downTop)
                            {
                                Gizmos.color = lineColor;
                            }
                            else if (topDown)
                            {
                                Gizmos.color = this.descentOnlyColor;
                            }
                            else if (downTop)
                            {
                                Gizmos.color = this.ascentOnlyColor;
                            }
                            else
                            {
                                continue;
                            }

                            Gizmos.DrawLine(c.position + heightAdj, other.position + heightAdj);
                        }
                    } /* end for each selected neighbour */
                }
            }

            return true;
        }

        private void Update()
        {
            if (Application.isEditor && !Application.isPlaying)
            {
                _nextRefresh = DateTime.UtcNow.AddMilliseconds(this.editorRefreshDelay);
            }
        }

        private class ModelProps : IUnitProperties
        {
            private AttributeMask _attribs = AttributeMask.None;

            public float radius
            {
                get { return 0.5f; }
            }

            public float fieldOfView
            {
                get { return 200f; }
            }

            public float groundOffset
            {
                get { return 0f; }
            }

            public float baseToPositionOffset
            {
                get { return 0f; }
            }

            public float height
            {
                get { return 1f; }
            }

            public Vector3 basePosition
            {
                get { return Vector3.zero; }
            }

            public HeightNavigationCapabilities heightNavigationCapability
            {
                get;
                set;
            }

            public Vector3 position
            {
                get { return Vector3.zero; }
            }

            public AttributeMask attributes
            {
                get { return _attribs; }
                set { _attribs = value; }
            }

            public bool isSelectable
            {
                get { return false; }
            }

            public bool isSelected
            {
                get;
                set;
            }

            public int determination
            {
                get { return 1; }
                set { /* NOOP */ }
            }

            public void RecalculateBasePosition()
            {
            }

            public void MarkSelectPending(bool pending)
            {
                /* NOOP */
            }
        }
    }
}
