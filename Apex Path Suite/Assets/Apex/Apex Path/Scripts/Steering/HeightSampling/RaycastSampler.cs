/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Steering
{
    using Apex.Units;
    using Apex.Utilities;
    using Apex.WorldGeometry;
    using UnityEngine;

    public sealed class RaycastSampler : ISampleHeights
    {
        public HeightSamplingMode samplingStrategy
        {
            get { return HeightSamplingMode.Raycast; }
        }

        public float SampleHeight(Vector3 position)
        {
            var g = GridManager.instance.GetGrid(position);
            var matrix = g != null ? g.cellMatrix : null;

            return SampleHeight(position, matrix);
        }

        public float SampleHeight(Vector3 position, CellMatrix matrix)
        {
            float plotRange;

            if (matrix != null)
            {
                position.y = matrix.origin.y + matrix.upperBoundary;
                plotRange = matrix.upperBoundary + matrix.lowerBoundary;
            }
            else
            {
                plotRange = Mathf.Infinity;
            }

            RaycastHit hit;
            if (Physics.Raycast(position, Vector3.down, out hit, plotRange, Layers.terrain))
            {
                return hit.point.y;
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
            float plotRange;

            if (matrix != null)
            {
                position.y = matrix.origin.y + matrix.upperBoundary;
                plotRange = matrix.upperBoundary + matrix.lowerBoundary;

                if (!matrix.bounds.Contains(position))
                {
                    height = Consts.InfiniteDrop;
                    return false;
                }
            }
            else
            {
                plotRange = Mathf.Infinity;
            }

            RaycastHit hit;
            if (Physics.Raycast(position, Vector3.down, out hit, plotRange, Layers.terrain))
            {
                height = hit.point.y;
                return true;
            }

            height = Consts.InfiniteDrop;
            return false;
        }
    }
}
