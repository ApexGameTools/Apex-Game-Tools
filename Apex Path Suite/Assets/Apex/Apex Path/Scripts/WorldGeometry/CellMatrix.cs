/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Apex.DataStructures;
    using Apex.Services;
    using Apex.Steering;
    using Apex.Utilities;
    using UnityEngine;

    /// <summary>
    /// Matrix of <see cref="Cell"/>s.
    /// </summary>
    public class CellMatrix : Matrix<Cell>, IHeightMap, ISampleHeightsSimple
    {
        private Vector3 _origin;
        private Vector3 _start;
        private Bounds _bounds;
        private float _cellSize;
        private float _obstacleSensitivityRange;
        private bool _generateHeightMap;
        private float _upperBoundary;
        private float _lowerBoundary;
        private ColliderDetectionMode _obstacleAndGroundDetection;
        private float _granularity;
        private int _heightMapSizeX;
        private int _heightMapSizeZ;
        private HeightLookupType _heightLookupType;
        private int _heightLookupMaxDepth;
        private IHeightLookup _heightLookup;
        private IList<PortalCell> _shortcutPortals;
        private IBlockDetector _obstacleAndGroundDetector;

        /// <summary>
        /// Initializes a new instance of the <see cref="CellMatrix"/> class.
        /// </summary>
        /// <param name="cfg">The configuration.</param>
        protected CellMatrix(ICellMatrixConfiguration cfg)
            : base(cfg.sizeX, cfg.sizeZ)
        {
            Initialize(cfg);
        }

        /// <summary>
        /// Gets the bounds of the matrix.
        /// </summary>
        /// <value>
        /// The bounds.
        /// </value>
        public Bounds bounds
        {
            get { return _bounds; }
        }

        bool IHeightMap.isGridBound
        {
            get { return true; }
        }

        /// <summary>
        /// Gets the granularity of the height map, i.e. the distance between height samples.
        /// </summary>
        /// <value>
        /// The granularity.
        /// </value>
        public float granularity
        {
            get { return _granularity; }
        }

        internal Vector3 origin
        {
            get { return _origin; }
        }

        internal Vector3 start
        {
            get { return _start; }
        }

        internal float upperBoundary
        {
            get { return _upperBoundary; }
        }

        internal float lowerBoundary
        {
            get { return _lowerBoundary; }
        }

        internal float cellSize
        {
            get { return _cellSize; }
        }

        internal bool hasHeightMap
        {
            get { return _heightLookup.hasHeights; }
        }

        internal VectorXZ heightMapSize
        {
            get { return new VectorXZ(_heightMapSizeX, _heightMapSizeZ); }
        }

        internal int heightMapEntries
        {
            get { return _heightLookup.heightCount; }
        }

        internal IList<PortalCell> shortcutPortals
        {
            get { return _shortcutPortals; }
        }

        internal float obstacleSensitivityRange
        {
            get { return _obstacleSensitivityRange; }
        }

        /// <summary>
        /// Creates a cell matrix based on the given configuration.
        /// </summary>
        /// <param name="cfg">The configuration.</param>
        /// <returns>The matrix</returns>
        public static CellMatrix Create(ICellMatrixConfiguration cfg)
        {
            var matrix = new CellMatrix(cfg);

            var iter = matrix.Populate();
            while (iter.MoveNext())
            {
                /* NOOP, we just iterate over all */
            }

            return matrix;
        }

        /// <summary>
        /// Creates a cell matrix based on the given configuration and stored data.
        /// </summary>
        /// <param name="cfg">The configuration.</param>
        /// <param name="data">The data.</param>
        /// <returns>The matrix</returns>
        public static CellMatrix Create(ICellMatrixConfiguration cfg, CellMatrixData data)
        {
            var matrix = new CellMatrix(cfg);

            var iter = matrix.Populate(data);
            while (iter.MoveNext())
            {
                /* NOOP, we just iterate over all */
            }

            return matrix;
        }

        internal static CellMatrix CreateForEditor(ICellMatrixConfiguration cfg, CellMatrixData data)
        {
            var matrix = new CellMatrix(cfg);

            if (data == null)
            {
                matrix._heightLookup = matrix.CreateHeightLookup(0);
            }
            else
            {
                matrix.InitHeightMapForEditor(data);
            }

            return matrix;
        }

        internal static MatrixIncrementalInitializer CreateIncrementally(ICellMatrixConfiguration cfg)
        {
            return new MatrixIncrementalInitializer(cfg);
        }

        internal static MatrixIncrementalInitializer CreateIncrementally(ICellMatrixConfiguration cfg, CellMatrixData data)
        {
            return new MatrixIncrementalInitializer(cfg, data);
        }

        /// <summary>
        /// Determines whether the bounds of the matrix contains the specified position.
        /// </summary>
        /// <param name="pos">The position.</param>
        /// <returns>
        ///   <c>true</c> if the position is contained; otherwise false.
        /// </returns>
        public bool Contains(Vector3 pos)
        {
            return _bounds.Contains(pos);
        }

        float ISampleHeightsSimple.SampleHeight(Vector3 position, CellMatrix matrix)
        {
            var fx = (position.x - _start.x) / _granularity;
            var fz = (position.z - _start.z) / _granularity;

            var x = Mathf.RoundToInt(fx);
            var z = Mathf.RoundToInt(fz);

            float height;
            if (_heightLookup.TryGetHeight(x, z, out height))
            {
                return height;
            }

            return this.origin.y;
        }

        /// <summary>
        /// Samples the height at the specified position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns>
        /// The height at the position
        /// </returns>
        float IHeightMap.SampleHeight(Vector3 position)
        {
            if (!_bounds.Contains(position))
            {
                return Consts.InfiniteDrop;
            }

            var fx = (position.x - _start.x) / _granularity;
            var fz = (position.z - _start.z) / _granularity;

            var x = Mathf.RoundToInt(fx);
            var z = Mathf.RoundToInt(fz);

            float height;
            if (_heightLookup.TryGetHeight(x, z, out height))
            {
                return height;
            }

            return this.origin.y;
        }

        /// <summary>
        /// Renders the height overlay for height map storage visualization.
        /// </summary>
        /// <param name="drawColor">Color of the draw.</param>
        public void RenderHeightOverlay(Color drawColor)
        {
            if (_heightLookup != null)
            {
                _heightLookup.Render(_start, _granularity, drawColor);
            }
        }

        /// <summary>
        /// Tries to sample the height at the specified position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="height">The height at the position.</param>
        /// <returns><c>true</c> if the position is covered by the height map and a height could be found; otherwise <c>false</c></returns>
        bool IHeightMap.TrySampleHeight(Vector3 position, out float height)
        {
            if (!_bounds.Contains(position))
            {
                height = Consts.InfiniteDrop;
                return false;
            }

            var fx = (position.x - _start.x) / _granularity;
            var fz = (position.z - _start.z) / _granularity;

            var x = Mathf.RoundToInt(fx);
            var z = Mathf.RoundToInt(fz);

            if (_heightLookup.TryGetHeight(x, z, out height))
            {
                return true;
            }

            height = this.origin.y;
            return true;
        }

        internal float SampleHeight(int x, int z)
        {
            float height;
            if (_heightLookup.TryGetHeight(x, z, out height))
            {
                return height;
            }

            return this.origin.y;
        }

        internal Cell GetCell(Vector3 position, bool adjustToBounds)
        {
            var fx = (position.x - _start.x) / _cellSize;
            var fz = (position.z - _start.z) / _cellSize;

            var x = Mathf.FloorToInt(fx);
            var z = Mathf.FloorToInt(fz);

            //This is to handle the corner case where the position is on the exact right or top border
            if (!adjustToBounds && !InBounds(x, z) && _bounds.Contains(position))
            {
                adjustToBounds = true;
            }

            if (adjustToBounds)
            {
                x = AdjustColumnToBounds(x);
                z = AdjustRowToBounds(z);

                return this.rawMatrix[x, z];
            }

            return this[x, z];
        }

        internal MatrixBounds GetMatrixBounds(Bounds b, float minOverlapToInclude, bool adjustToBounds)
        {
            return GetMatrixBounds(b.center, b.extents.x, b.extents.z, minOverlapToInclude, adjustToBounds);
        }

        internal MatrixBounds GetMatrixBounds(Vector3 position, float radiusX, float radiusZ, float minOverlapToInclude, bool adjustToBounds)
        {
            float maxAdj = 0f;
            float minAdj = 0f;
            if (minOverlapToInclude != 0.0f)
            {
                //We only want to include a cell when it is overlapped by a certain amount. The minAdj is to cater for borderline cases, i.e. when radius exactly matches the overlap
                radiusX -= minOverlapToInclude;
                radiusZ -= minOverlapToInclude;
                minAdj = 0.00001f;
            }
            else
            {
                //We do not want to include a cell in the borderline case, i.e. when radius hits right on a cell boundary. If not adjusted cells along the positive axis will include the next cell in such cases.
                maxAdj = 0.00001f;
            }

            if (adjustToBounds)
            {
                return new MatrixBounds
                {
                    minColumn = AdjustColumnToBounds(Mathf.FloorToInt((position.x - _start.x - radiusX - minAdj) / _cellSize)),
                    minRow = AdjustRowToBounds(Mathf.FloorToInt((position.z - _start.z - radiusZ - minAdj) / _cellSize)),
                    maxColumn = AdjustColumnToBounds(Mathf.FloorToInt((position.x - _start.x + radiusX - maxAdj) / _cellSize)),
                    maxRow = AdjustRowToBounds(Mathf.FloorToInt((position.z - _start.z + radiusZ - maxAdj) / _cellSize))
                };
            }

            return new MatrixBounds
            {
                minColumn = Mathf.FloorToInt((position.x - _start.x - radiusX - minAdj) / _cellSize),
                minRow = Mathf.FloorToInt((position.z - _start.z - radiusZ - minAdj) / _cellSize),
                maxColumn = Mathf.FloorToInt((position.x - _start.x + radiusX - maxAdj) / _cellSize),
                maxRow = Mathf.FloorToInt((position.z - _start.z + radiusZ - maxAdj) / _cellSize)
            };
        }

        internal IEnumerator Update(Bounds extent)
        {
            if (_generateHeightMap)
            {
                //First update the height map
                var min = extent.min;
                var max = extent.max;

                var updateBounds = new MatrixBounds(
                    Mathf.Clamp(Mathf.FloorToInt((min.x - _start.x) / _granularity), 0, _heightMapSizeX - 1),
                    Mathf.Clamp(Mathf.FloorToInt((min.z - _start.z) / _granularity), 0, _heightMapSizeZ - 1),
                    Mathf.Clamp(Mathf.CeilToInt((max.x - _start.x) / _granularity), 0, _heightMapSizeX - 1),
                    Mathf.Clamp(Mathf.CeilToInt((max.z - _start.z) / _granularity), 0, _heightMapSizeZ - 1));

                var heightUpdater = _heightLookup.PrepareForUpdate(updateBounds, out updateBounds);

                var detailedHeightMap = (GameServices.heightStrategy.heightMapDetail == HeightMapDetailLevel.High);
                var plotRange = _lowerBoundary + _upperBoundary;
                var down = Vector3.down;

                Vector3 samplePos;
                samplePos.y = _start.y + _upperBoundary;

                RaycastHit hit;
                for (int x = updateBounds.minColumn; x <= updateBounds.maxColumn; x++)
                {
                    samplePos.x = detailedHeightMap ? (float)Math.Round(_start.x + (x * _granularity), 4) : _start.x + (x * _granularity);

                    for (int z = updateBounds.minRow; z <= updateBounds.maxRow; z++)
                    {
                        samplePos.z = detailedHeightMap ? (float)Math.Round(_start.z + (z * _granularity), 4) : _start.z + (z * _granularity);

                        if (UnityServices.physics.Raycast(samplePos, down, out hit, plotRange, Layers.terrain))
                        {
                            heightUpdater.Add(x, z, hit.point.y);
                        }
                        else
                        {
                            heightUpdater.Add(x, z, Consts.InfiniteDrop);
                        }

                        yield return null;
                    }
                }

                _heightLookup.FinishUpdate(heightUpdater);
            }

            //Next update the affected grid cells
            var heightSampler = GetHeightSampler();

            var bounds = GetMatrixBounds(extent.center, extent.extents.x, extent.extents.z, 0.0f, true);

            var blockThreshold = Mathf.Max(_obstacleSensitivityRange, 0.01f);
            var maxHeight = _origin.y + _upperBoundary;
            var minHeight = _origin.y - _lowerBoundary;
            for (int x = bounds.minColumn; x <= bounds.maxColumn; x++)
            {
                for (int z = bounds.minRow; z <= bounds.maxRow; z++)
                {
                    var cell = this.rawMatrix[x, z];

                    var cellPos = cell.position;
                    cellPos.y = Mathf.Clamp(heightSampler.SampleHeight(cellPos, this), minHeight, maxHeight);

                    var blocked = IsBlocked(cellPos, blockThreshold);

                    cell.UpdateState(cellPos.y, blocked);

                    yield return null;
                }
            }

            var cellConstruct = GameServices.cellConstruction;
            if (cellConstruct.calculateHeights)
            {
                var iter = cellConstruct.heightSettingsProvider.AssignHeightSettings(this, bounds);
                while (iter.MoveNext())
                {
                    yield return null;
                }
            }

            //Clearance cannot be partially updated so we have to rescan the entire grid
            if (cellConstruct.clearanceProvider != null)
            {
                var iter = cellConstruct.clearanceProvider.Reset(this);
                while (iter.MoveNext())
                {
                    yield return null;
                }

                iter = cellConstruct.clearanceProvider.SetClearance(this);
                while (iter.MoveNext())
                {
                    yield return null;
                }
            }
        }

        internal void UpdateForEditor(Bounds extent, CellMatrixData data)
        {
            var bounds = GetMatrixBounds(extent.center, extent.extents.x + (_cellSize * 2), extent.extents.z + (_cellSize * 2), 0.0f, true);
            var cellFactory = GameServices.cellConstruction.cellFactory;

            if (data == null)
            {
                var blockThreshold = Mathf.Max(_obstacleSensitivityRange, 0.01f);
                for (int x = bounds.minColumn; x <= bounds.maxColumn; x++)
                {
                    for (int z = bounds.minRow; z <= bounds.maxRow; z++)
                    {
                        var cell = this.rawMatrix[x, z];

                        if (cell == null)
                        {
                            var position = new Vector3(_start.x + (x * _cellSize) + (_cellSize / 2.0f), _origin.y, _start.z + (z * _cellSize) + (_cellSize / 2.0f));

                            var blocked = IsBlocked(position, blockThreshold);

                            this.rawMatrix[x, z] = cellFactory.Create(this, position, x, z, blocked);
                        }
                        else
                        {
                            var cellPos = cell.position;
                            var blocked = IsBlocked(cellPos, blockThreshold);

                            cell.UpdateState(cellPos.y, blocked);
                        }
                    }
                }
            }
            else
            {
                var accessor = data.GetAccessor();

                var heightSampler = GetHeightSampler();
                var maxHeight = _origin.y + _upperBoundary;
                var minHeight = _origin.y - _lowerBoundary;

                //Init the cells
                for (int x = bounds.minColumn; x <= bounds.maxColumn; x++)
                {
                    for (int z = bounds.minRow; z <= bounds.maxRow; z++)
                    {
                        var cell = this.rawMatrix[x, z];

                        if (cell == null)
                        {
                            var idx = (z * this.columns) + x;

                            var position = new Vector3(_start.x + (x * _cellSize) + (_cellSize / 2.0f), _origin.y, _start.z + (z * _cellSize) + (_cellSize / 2.0f));
                            position.y = Mathf.Clamp(heightSampler.SampleHeight(position, this), minHeight, maxHeight);

                            var blocked = accessor.IsPermaBlocked(idx);

                            cell = cellFactory.Create(this, position, x, z, blocked);
                            accessor.InjectData(cell, idx);

                            this.rawMatrix[x, z] = cell;

                            cell.EnsureDataForEditor();
                        }
                    }
                }
            }
        }

        private void InitHeightMapForEditor(CellMatrixData data)
        {
            var accessor = data.GetAccessor();

            //Get the height map
            _heightLookup = CreateHeightLookup(accessor.heightEntries);

            for (int x = 0; x < _heightMapSizeX; x++)
            {
                for (int z = 0; z < _heightMapSizeZ; z++)
                {
                    var idx = (x * _heightMapSizeZ) + z;
                    var height = accessor.GetHeight(idx);

                    _heightLookup.Add(x, z, height);
                }
            }

            _heightLookup.Cleanup();
        }

        private void Initialize(ICellMatrixConfiguration cfg)
        {
            _cellSize = cfg.cellSize;
            _origin = cfg.origin;
            _start = cfg.origin;
            _start.x -= this.columns * 0.5f * _cellSize;
            _start.z -= this.rows * 0.5f * _cellSize;

            _obstacleSensitivityRange = cfg.obstacleSensitivityRange;
            _generateHeightMap = cfg.generateHeightmap && (GameServices.heightStrategy.heightMode == HeightSamplingMode.HeightMap);
            _upperBoundary = cfg.upperBoundary;
            _lowerBoundary = cfg.lowerBoundary;
            _obstacleAndGroundDetection = cfg.obstacleAndGroundDetection;
            _obstacleAndGroundDetector = cfg.obstacleAndGroundDetector;

            _granularity = GameServices.heightStrategy.sampleGranularity;

            if (_generateHeightMap)
            {
                _heightMapSizeX = Mathf.RoundToInt(this.columns * this.cellSize / _granularity) + 1;
                _heightMapSizeZ = Mathf.RoundToInt(this.rows * this.cellSize / _granularity) + 1;
                _heightLookupType = cfg.heightLookupType;
                _heightLookupMaxDepth = cfg.heightLookupMaxDepth;
            }

            _bounds = cfg.bounds;

            _shortcutPortals = new List<PortalCell>();
        }

        private IEnumerator Populate()
        {
            //Create the height map
            _heightLookup = CreateHeightLookup(0);

            if (_generateHeightMap)
            {
                var detailedHeightMap = (GameServices.heightStrategy.heightMapDetail == HeightMapDetailLevel.High);
                var plotRange = _lowerBoundary + _upperBoundary;
                var down = Vector3.down;

                Vector3 samplePos;
                samplePos.y = _start.y + _upperBoundary;

                RaycastHit hit;
                for (int x = 0; x < _heightMapSizeX; x++)
                {
                    samplePos.x = detailedHeightMap ? (float)Math.Round(_start.x + (x * _granularity), 4) : _start.x + (x * _granularity);

                    for (int z = 0; z < _heightMapSizeZ; z++)
                    {
                        samplePos.z = detailedHeightMap ? (float)Math.Round(_start.z + (z * _granularity), 4) : _start.z + (z * _granularity);

                        if (UnityServices.physics.Raycast(samplePos, down, out hit, plotRange, Layers.terrain))
                        {
                            _heightLookup.Add(x, z, hit.point.y);
                        }
                        else
                        {
                            _heightLookup.Add(x, z, Consts.InfiniteDrop);
                        }

                        yield return null;
                    }
                }

                _heightLookup.Cleanup();
            }

            //Populate the cell matrix
            var heightSampler = GetHeightSampler();
            var cellConstruct = GameServices.cellConstruction;
            var cellFactory = cellConstruct.cellFactory;
            var calculateHeights = cellConstruct.calculateHeights;

            var maxHeight = _origin.y + _upperBoundary;
            var minHeight = _origin.y - _lowerBoundary;
            var blockThreshold = Mathf.Max(_obstacleSensitivityRange, 0.01f);
            for (int x = 0; x < this.columns; x++)
            {
                for (int z = 0; z < this.rows; z++)
                {
                    var position = new Vector3(_start.x + (x * _cellSize) + (_cellSize / 2.0f), _origin.y, _start.z + (z * _cellSize) + (_cellSize / 2.0f));
                    if (calculateHeights)
                    {
                        position.y = Mathf.Clamp(heightSampler.SampleHeight(position, this), minHeight, maxHeight);
                    }

                    var blocked = IsBlocked(position, blockThreshold);

                    this.rawMatrix[x, z] = cellFactory.Create(this, position, x, z, blocked);

                    yield return null;
                }
            }

            //Set the height block status of each cell
            if (calculateHeights)
            {
                var entireMatrix = new MatrixBounds(0, 0, this.columns - 1, this.rows - 1);

                //Set height map settings for all cells
                var iter = cellConstruct.heightSettingsProvider.AssignHeightSettings(this, entireMatrix);
                while (iter.MoveNext())
                {
                    yield return null;
                }
            }

            //Set clearance
            if (cellConstruct.clearanceProvider != null)
            {
                var iter = cellConstruct.clearanceProvider.SetClearance(this);
                while (iter.MoveNext())
                {
                    yield return null;
                }
            }
        }

        private IEnumerator Populate(CellMatrixData data)
        {
            var accessor = data.GetAccessor();

            //Get the height lookup
            _heightLookup = CreateHeightLookup(accessor.heightEntries);

            for (int x = 0; x < _heightMapSizeX; x++)
            {
                for (int z = 0; z < _heightMapSizeZ; z++)
                {
                    var idx = (x * _heightMapSizeZ) + z;
                    var height = accessor.GetHeight(idx);

                    _heightLookup.Add(x, z, height);

                    yield return null;
                }
            }

            _heightLookup.Cleanup();

            //Init the cells
            var heightSampler = GetHeightSampler();
            var cellFactory = GameServices.cellConstruction.cellFactory;
            var maxHeight = _origin.y + _upperBoundary;
            var minHeight = _origin.y - _lowerBoundary;

            for (int x = 0; x < this.columns; x++)
            {
                for (int z = 0; z < this.rows; z++)
                {
                    var idx = (z * this.columns) + x;

                    var position = new Vector3(_start.x + (x * _cellSize) + (_cellSize / 2.0f), _origin.y, _start.z + (z * _cellSize) + (_cellSize / 2.0f));
                    position.y = Mathf.Clamp(heightSampler.SampleHeight(position, this), minHeight, maxHeight);

                    var blocked = accessor.IsPermaBlocked(idx);

                    var cell = cellFactory.Create(this, position, x, z, blocked);
                    accessor.InjectData(cell, idx);

                    this.rawMatrix[x, z] = cell;

                    yield return null;
                }
            }

            //For now we don't bake clearance data
            var cellConstruct = GameServices.cellConstruction;
            if (cellConstruct.clearanceProvider != null)
            {
                var iter = cellConstruct.clearanceProvider.SetClearance(this);
                while (iter.MoveNext())
                {
                    yield return null;
                }
            }
        }

        private ISampleHeightsSimple GetHeightSampler()
        {
            if (GameServices.heightStrategy.heightMode == HeightSamplingMode.HeightMap)
            {
                return this;
            }

            return GameServices.heightStrategy.heightSampler;
        }

        private IHeightLookup CreateHeightLookup(int heightEntries)
        {
            if (_heightLookupType == HeightLookupType.Dictionary)
            {
                return new HeightDictionary(_heightMapSizeX, _heightMapSizeZ, heightEntries, _origin.y);
            }

            return new HeightQuadTree(_heightMapSizeX, _heightMapSizeZ, _heightLookupMaxDepth, _origin.y);
        }

        private bool IsBlocked(Vector3 position, float blockThreshold)
        {
            //Be sure to check from the origin, not from the height of the position.
            position.y = _origin.y;

            switch (_obstacleAndGroundDetection)
            {
                case ColliderDetectionMode.Mixed:
                {
                    var above = position + (Vector3.up * (_upperBoundary - blockThreshold));
                    var below = position + (Vector3.down * (_lowerBoundary - blockThreshold));

                    if (UnityServices.physics.CheckCapsule(above, below, blockThreshold, Layers.blocks))
                    {
                        return true;
                    }

                    //Capsule check is bugged so for many this works better
                    var distance = (_upperBoundary + _lowerBoundary);
                    above = position + (Vector3.up * (_upperBoundary + 0.01f));
                    var r = new Ray(above, Vector3.down);

                    return !UnityServices.physics.SphereCast(r, 0.01f, distance, Layers.terrain);
                }

                case ColliderDetectionMode.CheckCapsule:
                {
                    var above = position + (Vector3.up * (_upperBoundary - blockThreshold));
                    var below = position + (Vector3.down * (_lowerBoundary - blockThreshold));

                    if (UnityServices.physics.CheckCapsule(above, below, blockThreshold, Layers.blocks))
                    {
                        return true;
                    }

                    //Doing a simple raycast does not cut it in scenarios where level geometry is made up from multiple pieces, as the ray can slip through the most minuscule of cracks
                    //Luckily capsule check is just as fast
                    above = position + (Vector3.up * (_upperBoundary - 0.01f));
                    below = position + (Vector3.down * (_lowerBoundary - 0.01f));

                    return !UnityServices.physics.CheckCapsule(above, below, 0.01f, Layers.terrain);
                }

                case ColliderDetectionMode.Custom:
                {
                    return _obstacleAndGroundDetector.IsBlocked(this, position, blockThreshold);
                }

                default:
                case ColliderDetectionMode.SphereCast:
                {
                    //Sphere cast for both
                    var distance = (_upperBoundary + _lowerBoundary);

                    var above = position + (Vector3.up * (_upperBoundary + blockThreshold));
                    Ray r = new Ray(above, Vector3.down);

                    if (UnityServices.physics.SphereCast(r, blockThreshold, distance, Layers.blocks))
                    {
                        return true;
                    }

                    above = position + (Vector3.up * (_upperBoundary + 0.01f));
                    r = new Ray(above, Vector3.down);

                    return !UnityServices.physics.SphereCast(r, 0.01f, distance, Layers.terrain);
                }
            }
        }

        internal class MatrixIncrementalInitializer
        {
            private CellMatrix _matrix;
            private IEnumerator _iter;

            internal MatrixIncrementalInitializer(ICellMatrixConfiguration cfg, CellMatrixData data)
            {
                _matrix = new CellMatrix(cfg);
                _iter = _matrix.Populate(data);
            }

            internal MatrixIncrementalInitializer(ICellMatrixConfiguration cfg)
            {
                _matrix = new CellMatrix(cfg);
                _iter = _matrix.Populate();
            }

            internal bool isInitializing
            {
                get
                {
                    var moreWork = _iter.MoveNext();
                    if (!moreWork)
                    {
                        this.matrix = _matrix;
                    }

                    return moreWork;
                }
            }

            internal CellMatrix matrix
            {
                get;
                private set;
            }
        }
    }
}
