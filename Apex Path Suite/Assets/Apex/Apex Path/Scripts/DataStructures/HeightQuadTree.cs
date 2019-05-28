/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.DataStructures
{
    using System;
    using System.Collections.Generic;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// Internal ADT for height nav manangement.
    /// </summary>
    public class HeightQuadTree : IHeightLookup
    {
        private int _depth;
        private int _maxDepth;
        private float _height;
        private DynamicArray<VectorXZ> _indexes;
        private Dictionary<int, float> _lookup;
        private MatrixBounds _bounds;
        private HeightQuadTree[] _subTrees;

        public HeightQuadTree(int sizeX, int sizeZ, int maxDepth, float originY)
            : this(0, 0, sizeX, sizeZ, 0, maxDepth)
        {
            _height = originY;
        }

        private HeightQuadTree(int startX, int startZ, int sizeX, int sizeZ, int depth, int maxDepth)
        {
            _bounds = new MatrixBounds(startX, startZ, startX + (sizeX - 1), startZ + (sizeZ - 1));
            _depth = depth;
            _maxDepth = maxDepth;

            _indexes = new DynamicArray<VectorXZ>(10);
        }

        private HeightQuadTree(MatrixBounds bounds, int depth, int maxDepth)
        {
            _bounds = bounds;
            _depth = depth;
            _maxDepth = maxDepth;

            _indexes = new DynamicArray<VectorXZ>(10);
        }

        public bool hasHeights
        {
            get { return ((_subTrees != null) || (_depth > 0)); }
        }

        public int heightCount
        {
            get { return _bounds.columns * _bounds.rows; }
        }

        private bool canSplit
        {
            get
            {
                if (_depth >= _maxDepth)
                {
                    return false;
                }

                return (_bounds.rows > 1) && (_bounds.columns > 1);
            }
        }

        public IHeightLookup PrepareForUpdate(MatrixBounds suggestedBounds, out MatrixBounds requiredBounds)
        {
            if (!_bounds.Contains(suggestedBounds))
            {
                throw new ArgumentException("Cannot update a quadtree of a smaller size than requested bounds.", "suggestedBounds");
            }

            var updater = GetUpdateTree(null, 0, suggestedBounds);

            if (updater != null)
            {
                requiredBounds = updater.bounds;
                return updater;
            }

            //The update covers an area that requires a full reindexing.
            //This can happen with even very small areas, they just need to overlap one of the borders between subtrees.
            //And this is why dynamic updates are not friends with quad trees
            requiredBounds = _bounds;
            _lookup = null;
            _subTrees = null;
            _indexes = new DynamicArray<VectorXZ>(10);
            return this;
        }

        public void FinishUpdate(IHeightLookup updatedHeights)
        {
            var updateTree = updatedHeights as UpdatingHeightTree;
            if (updateTree != null)
            {
                updateTree.Inject();
            }
        }

        private UpdatingHeightTree GetUpdateTree(HeightQuadTree parent, int parentIdx, MatrixBounds suggestedBounds)
        {
            if (_subTrees != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (_subTrees[i]._bounds.Contains(suggestedBounds))
                    {
                        return _subTrees[i].GetUpdateTree(this, i, suggestedBounds);
                    }
                }
            }

            if (parent != null)
            {
                var freshStart = new HeightQuadTree(_bounds, _depth, _maxDepth);
                return new UpdatingHeightTree(freshStart, parent, parentIdx);
            }

            return null;
        }

        public bool Add(int x, int z, float height)
        {
            if (!_bounds.Contains(x, z))
            {
                return false;
            }

            if (_lookup != null)
            {
                var key = (x * _bounds.rows) + z;
                _lookup[key] = height;
                return true;
            }

            if (_subTrees != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (_subTrees[i].Add(x, z, height))
                    {
                        return true;
                    }
                }

                //not gonna happen but...
                return false;
            }

            if (_height == height)
            {
                _indexes.Add(new VectorXZ(x, z));
            }
            else if (_indexes.count == 0)
            {
                _height = height;
                _indexes.Add(new VectorXZ(x, z));
            }
            else if (this.canSplit)
            {
                SplitToSubTrees(x, z, height);
            }
            else
            {
                CreateLookup(x, z, height);
            }

            return true;
        }

        public bool TryGetHeight(int x, int z, out float height)
        {
            if (!_bounds.Contains(x, z))
            {
                height = 0f;
                return false;
            }

            if (_lookup != null)
            {
                var key = (x * _bounds.rows) + z;
                return _lookup.TryGetValue(key, out height);
            }

            if (_subTrees != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (_subTrees[i].TryGetHeight(x, z, out height))
                    {
                        return true;
                    }
                }
            }

            height = _height;
            return true;
        }

        public void Cleanup()
        {
            _indexes = null;
            if (_subTrees != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    _subTrees[i].Cleanup();
                }
            }
        }

        public void Render(Vector3 position, float pointGranularity, Color drawColor)
        {
            if (_subTrees != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    _subTrees[i].Render(position, pointGranularity, drawColor);
                }

                return;
            }

            Bounds b = new Bounds();
            b.SetMinMax(
                position + new Vector3(_bounds.minColumn * pointGranularity, 5f, _bounds.minRow * pointGranularity),
                position + new Vector3(_bounds.maxColumn * pointGranularity, 5f, _bounds.maxRow * pointGranularity));

            Gizmos.color = drawColor;
            if (_lookup != null)
            {
                Gizmos.DrawCube(b.center, b.size);
            }
            else
            {
                Gizmos.DrawWireCube(b.center, b.size);
            }
        }

        private void SplitToSubTrees(int newX, int newZ, float newHeight)
        {
            _subTrees = new HeightQuadTree[4];

            var sizeXLeft = _bounds.columns / 2;
            var sizeXRight = _bounds.columns - sizeXLeft;
            var sizeZBottom = _bounds.rows / 2;
            var sizeZTop = _bounds.rows - sizeZBottom;
            var subDepth = _depth + 1;
            _subTrees[0] = new HeightQuadTree(_bounds.minColumn, _bounds.minRow, sizeXLeft, sizeZBottom, subDepth, _maxDepth);
            _subTrees[1] = new HeightQuadTree(_bounds.minColumn + sizeXLeft, _bounds.minRow, sizeXRight, sizeZBottom, subDepth, _maxDepth);
            _subTrees[2] = new HeightQuadTree(_bounds.minColumn, _bounds.minRow + sizeZBottom, sizeXLeft, sizeZTop, subDepth, _maxDepth);
            _subTrees[3] = new HeightQuadTree(_bounds.minColumn + sizeXLeft, _bounds.minRow + sizeZBottom, sizeXRight, sizeZTop, subDepth, _maxDepth);

            var indexCount = _indexes.count;
            for (int i = 0; i < indexCount; i++)
            {
                var entry = _indexes[i];
                for (int j = 0; j < 4; j++)
                {
                    if (_subTrees[j].Add(entry.x, entry.z, _height))
                    {
                        break;
                    }
                }
            }

            for (int j = 0; j < 4; j++)
            {
                if (_subTrees[j].Add(newX, newZ, newHeight))
                {
                    break;
                }
            }

            _indexes = null;
            _height = 0f;
        }

        private void CreateLookup(int newX, int newZ, float newHeight)
        {
            var indexCount = _indexes.count;
            _lookup = new Dictionary<int, float>(indexCount + 1);

            var key = (newX * _bounds.rows) + newZ;
            _lookup[key] = newHeight;

            for (int i = 0; i < indexCount; i++)
            {
                var index = _indexes[i];
                key = (index.x * _bounds.rows) + index.z;
                _lookup[key] = _height;
            }

            _indexes = null;
            _height = 0f;
        }

        private class UpdatingHeightTree : IHeightLookup
        {
            private HeightQuadTree _target;
            private HeightQuadTree _parent;
            private int _parentIdx;

            public UpdatingHeightTree(HeightQuadTree target, HeightQuadTree parent, int parentIdx)
            {
                _target = target;
                _parent = parent;
                _parentIdx = parentIdx;
            }

            public MatrixBounds bounds
            {
                get { return _target._bounds; }
            }

            public bool hasHeights
            {
                get { return true; }
            }

            public int heightCount
            {
                get { return 0; }
            }

            internal void Inject()
            {
                _target.Cleanup();
                _parent._subTrees[_parentIdx] = _target;
            }

            bool IHeightLookup.Add(int x, int z, float height)
            {
                return _target.Add(x, z, height);
            }

            bool IHeightLookup.TryGetHeight(int x, int z, out float height)
            {
                //Not meant for lookup
                throw new InvalidOperationException("This is a temporary construct used for dynamic updates and is not meant for lookups.");
            }

            void IHeightLookup.Cleanup()
            {
                _target.Cleanup();
            }

            void IHeightLookup.Render(Vector3 position, float pointGranularity, Color drawColor)
            {
                throw new InvalidOperationException("This is a temporary construct used for dynamic updates and is not meant for rendering.");
            }

            IHeightLookup IHeightLookup.PrepareForUpdate(MatrixBounds suggestedBounds, out MatrixBounds requiredBounds)
            {
                throw new InvalidOperationException("This is a temporary construct used for dynamic updates only.");
            }

            void IHeightLookup.FinishUpdate(IHeightLookup updatedHeights)
            {
                throw new InvalidOperationException("This is a temporary construct used for dynamic updates only.");
            }
        }
    }
}
