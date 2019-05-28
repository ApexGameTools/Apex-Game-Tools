/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Steering
{
    using Apex.Units;
    using Apex.Utilities;
    using Apex.WorldGeometry;
    using UnityEngine;

    public sealed class NullSampler : ISampleHeights
    {
        public HeightSamplingMode samplingStrategy
        {
            get { return HeightSamplingMode.NoHeightSampling; }
        }

        public float SampleHeight(Vector3 position)
        {
            var g = GridManager.instance.GetGrid(position);
            var matrix = g != null ? g.cellMatrix : null;
            return SampleHeight(position, matrix);
        }

        public float SampleHeight(Vector3 position, CellMatrix matrix)
        {
            if (matrix != null)
            {
                return matrix.origin.y;
            }

            return Consts.InfiniteDrop;
        }

        public bool TrySampleHeight(Vector3 position, out float height)
        {
            var g = GridManager.instance.GetGrid(position);
            var matrix = g != null ? g.cellMatrix : null;
            return TrySampleHeight(position, matrix, out height);
        }

        public bool TrySampleHeight(Vector3 position, CellMatrix matrix, out float height)
        {
            if (matrix != null)
            {
                height = matrix.origin.y;
                return true;
            }

            height = Consts.InfiniteDrop;
            return false;
        }
    }
}
