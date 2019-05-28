/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.DataStructures
{
    using System.Collections.Generic;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// Internal ADT for height nav manangement.
    /// </summary>
    public class HeightDictionary : IHeightLookup
    {
        private IDictionary<int, float> _lookup;
        private int _sizeX;
        private int _sizeZ;
        private float _originY;

        public HeightDictionary(int sizeX, int sizeZ, int heightEntryCount, float originY)
        {
            _lookup = new Dictionary<int, float>(heightEntryCount);
            _sizeX = sizeX;
            _sizeZ = sizeZ;
            _originY = originY;
        }

        public bool hasHeights
        {
            get { return _lookup.Count > 0; }
        }

        public int heightCount
        {
            get { return _lookup.Count; }
        }

        public bool Add(int x, int z, float height)
        {
            var idx = (x * _sizeZ) + z;

            if (height != _originY)
            {
                _lookup[idx] = height;
            }

            return true;
        }

        public bool TryGetHeight(int x, int z, out float height)
        {
            var idx = (x * _sizeZ) + z;

            return _lookup.TryGetValue(idx, out height);
        }

        public void Cleanup()
        {
            /* NOOP */
        }

        public IHeightLookup PrepareForUpdate(MatrixBounds suggestedBounds, out MatrixBounds requiredBounds)
        {
            requiredBounds = suggestedBounds;
            return this;
        }

        public void FinishUpdate(IHeightLookup updatedHeights)
        {
            /* NOOP */
        }

        public void Render(Vector3 position, float pointGranularity, Color drawColor)
        {
            Bounds b = new Bounds();
            b.SetMinMax(
                position + new Vector3(0f, 5f, 0f),
                position + new Vector3(_sizeX * pointGranularity, 5f, _sizeZ * pointGranularity));

            Gizmos.color = drawColor;
            Gizmos.DrawCube(b.center, b.size);
        }
    }
}
