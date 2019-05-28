/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Steering
{
    using System;
    using Apex.Units;
    using Apex.WorldGeometry;
    using UnityEngine;

    public sealed class HeightMapSampler : ISampleHeights
    {
        public HeightSamplingMode samplingStrategy
        {
            get { return HeightSamplingMode.HeightMap; }
        }

        public float SampleHeight(Vector3 position)
        {
            return SampleHeight(position, null);
        }

        public float SampleHeight(Vector3 position, CellMatrix matrix)
        {
            var heightMap = HeightMapManager.instance.GetHeightMap(position);

            return heightMap.SampleHeight(position);
        }

        public bool TrySampleHeight(Vector3 position, out float height)
        {
            return TrySampleHeight(position, null, out height);
        }

        public bool TrySampleHeight(Vector3 position, CellMatrix matrix, out float height)
        {
            var heightMap = HeightMapManager.instance.GetHeightMap(position);

            return heightMap.TrySampleHeight(position, out height);
        }
    }
}
