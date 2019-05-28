/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    using UnityEngine;

    /// <summary>
    /// Represent a <see cref="IGrid"/> perimeter
    /// </summary>
    public sealed class Perimeter
    {
        internal Perimeter(Vector3 edgeVector, float edge, float edgeMid)
        {
            this.edgeVector = edgeVector;
            this.edge = edge;
            this.edgeMid = edgeMid;
        }

        internal float edge { get; private set; }

        internal float edgeMid { get; private set; }

        internal Vector3 edgeVector { get; private set; }

        internal Vector3 insideCornerOne { get; private set; }

        internal Vector3 insideCornerTwo { get; private set; }

        internal Vector3 outsideCornerOne { get; private set; }

        internal Vector3 outsideCornerTwo { get; private set; }

        internal Perimeter perpendicularOne { get; private set; }

        internal Perimeter perpendicularTwo { get; private set; }

        internal void SetPerpendiculars(Perimeter perpOne, Perimeter perpTwo, float step)
        {
            this.perpendicularOne = perpOne;
            this.perpendicularTwo = perpTwo;

            this.insideCornerOne = GetPoint(perpOne.edgeMid);
            this.insideCornerTwo = GetPoint(perpTwo.edgeMid);

            this.outsideCornerOne = this.insideCornerOne + ((this.edgeVector + perpOne.edgeVector) * step);
            this.outsideCornerTwo = this.insideCornerTwo + ((this.edgeVector + perpTwo.edgeVector) * step);
        }

        internal Vector3 GetPoint(Vector3 refPoint)
        {
            if (this.edgeVector.x != 0)
            {
                return new Vector3(this.edgeMid, 0.0f, refPoint.z);
            }

            return new Vector3(refPoint.x, 0.0f, this.edgeMid);
        }

        private Vector3 GetPoint(float otherAxis)
        {
            if (this.edgeVector.x != 0)
            {
                return new Vector3(this.edgeMid, 0.0f, otherAxis);
            }

            return new Vector3(otherAxis, 0.0f, this.edgeMid);
        }
    }
}
