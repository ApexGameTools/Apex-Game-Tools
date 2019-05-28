/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    using UnityEngine;

    /// <summary>
    /// Utility class exposing various geometry related functions
    /// </summary>
    public static class Geometry
    {
        /// <summary>
        /// Checks if two finite lines intersect
        /// </summary>
        /// <param name="line1p1">The first point on line one.</param>
        /// <param name="line1p2">The second point on line one.</param>
        /// <param name="line2p1">The first point on line one.</param>
        /// <param name="line2p2">The second point on line two.</param>
        /// <returns><c>true</c> if the line segments intersect, otherwise <c>false</c></returns>
        public static bool DoLinesIntersect(Vector3 line1p1, Vector3 line1p2, Vector3 line2p1, Vector3 line2p2)
        {
            var intersect = LineIntersection(line1p1, line1p2, line2p1, line2p2);

            if (!intersect.HasValue)
            {
                return false;
            }

            if (IsPointBetweenPointsX(intersect.Value, line1p1, line1p2))
            {
                return IsPointBetweenPointsX(intersect.Value, line2p1, line2p2);
            }

            return false;
        }

        /// <summary>
        /// Checks if two finite lines intersect
        /// </summary>
        /// <param name="line1p1">The first point on line one.</param>
        /// <param name="line1p2">The second point on line one.</param>
        /// <param name="line2p1">The first point on line one.</param>
        /// <param name="line2p2">The second point on line two.</param>
        /// <param name="intersectionPoint">Contains the intersectionPoint when one is found, otherwise null.</param>
        /// <returns><c>true</c> if the line segments intersect, otherwise <c>false</c></returns>
        public static bool DoLinesIntersect(Vector3 line1p1, Vector3 line1p2, Vector3 line2p1, Vector3 line2p2, out Vector3? intersectionPoint)
        {
            intersectionPoint = LineIntersection(line1p1, line1p2, line2p1, line2p2);

            if (!intersectionPoint.HasValue)
            {
                return false;
            }

            if (IsPointBetweenPointsX(intersectionPoint.Value, line1p1, line1p2))
            {
                return IsPointBetweenPointsX(intersectionPoint.Value, line2p1, line2p2);
            }

            return false;
        }

        /// <summary>
        /// Gets the intersection point between two infinite lines.
        /// </summary>
        /// <param name="line1p1">The first point on line one.</param>
        /// <param name="line1p2">The second point on line one.</param>
        /// <param name="line2p1">The first point on line one.</param>
        /// <param name="line2p2">The second point on line two.</param>
        /// <returns>The intersection point or null if the lines are parallel</returns>
        public static Vector3? LineIntersection(Vector3 line1p1, Vector3 line1p2, Vector3 line2p1, Vector3 line2p2)
        {
            var a1 = (line1p1.z - line1p2.z) / (line1p1.x - line1p2.x);
            var b1 = line1p1.z - (a1 * line1p1.x);

            var a2 = (line2p1.z - line2p2.z) / (line2p1.x - line2p2.x);
            var b2 = line2p1.z - (a2 * line2p1.x);

            if (a1 == a2)
            {
                return null;
            }

            var x = (b1 - b2) / (a2 - a1);
            var z = (a1 * x) + b1;

            return new Vector3(x, 0.0f, z);
        }

        public static bool DoesLineIntersectSphere(Vector3 linep1, Vector3 linep2, Vector3 sphereCenter, float sphereRadius)
        {
            var lineDir = (linep2 - linep1).normalized;
            var sphereToLineOrigin = linep1 - sphereCenter;

            var dot = Vector3.Dot(lineDir, sphereToLineOrigin);
            var q = (dot * dot) - sphereToLineOrigin.sqrMagnitude + (sphereRadius * sphereRadius);

            return q >= 0f;
        }

        private static bool IsPointBetweenPointsX(Vector3 point, Vector3 p1, Vector3 p2)
        {
            var maxX = Mathf.Max(p1.x, p2.x);
            var minX = Mathf.Min(p1.x, p2.x);

            return (point.x <= maxX) && (point.x >= minX);
        }
    }
}
