/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.PathFinding
{
    using System;
    using System.Collections.Generic;
    using Apex.DataStructures;
    using Apex.Utilities;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// Represents a path along which a unit can move.
    /// </summary>
    public class Path : StackWithLookAhead<IPositioned>, ICloneable
    {
        private float _length = -1f;

        /// <summary>
        /// Initializes a new instance of the <see cref="Path"/> class.
        /// </summary>
        public Path()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Path"/> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        public Path(int capacity)
            : base(capacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Path"/> class.
        /// </summary>
        /// <param name="path">The path points.</param>
        public Path(params Vector3[] path)
            : base(path.Length)
        {
            Ensure.ArgumentNotNull(path, "path");

            for (int i = path.Length - 1; i >= 0; i--)
            {
                Push(path[i].AsPositioned());
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Path"/> class.
        /// </summary>
        /// <param name="path">The path points.</param>
        public Path(IIndexable<Vector3> path)
            : base(path.count)
        {
            Ensure.ArgumentNotNull(path, "path");

            for (int i = path.count - 1; i >= 0; i--)
            {
                Push(path[i].AsPositioned());
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Path"/> class.
        /// </summary>
        /// <param name="path">The path points.</param>
        public Path(IIndexable<IPositioned> path)
            : base(path.count)
        {
            Ensure.ArgumentNotNull(path, "path");

            for (int i = path.count - 1; i >= 0; i--)
            {
                Push(path[i]);
            }
        }

        /// <summary>
        /// Gets the length of the path in meters. This is cached so it will only cause one call to <see cref="CalculateLength"/>
        /// </summary>
        /// <value>
        /// The length of the path.
        /// </value>
        public float length
        {
            get
            {
                if (_length < 0f)
                {
                    CalculateLength();
                }

                return _length;
            }
        }

        /// <summary>
        /// Concatenates multiple path segment, injecting waypoints at the end point of each.
        /// This method assumes that segments are like this: n1..nx, nx...ny, ny...nz, i.e. the last node of one segment is the first node of the next.
        /// </summary>
        /// <param name="segments">The segments.</param>
        /// <returns>The concatenated path.</returns>
        public static Path FromSegments(IIndexable<Path> segments)
        {
            Path segment;
            var count = segments.count;
            if (count == 0)
            {
                return null;
            }
            else if (count == 1)
            {
                segment = segments[0];
                segment._array[0] = new Waypoint(segment._array[0].position);
                return segment;
            }

            int size = 0;
            for (int i = 0; i < count; i++)
            {
                size += segments[i].count;
            }

            size = size - (count - 1);

            var result = new Path(size);
            result._used = size;
            var arr = result._array;

            var endPos = 0;            
            for (int i = count - 1; i >= 0; i--)
            {
                segment = segments[i];
                var segArr = segment._array;

                arr[endPos++] = new Waypoint(segArr[0].position);
                
                var lengthSub = (i == 0) ? 1 : 2;
                var remainder = segment.count - lengthSub;
                if (remainder > 0)
                {
                    Array.Copy(segArr, 1, arr, endPos, remainder);
                    endPos += remainder;
                }
            }

            return result;
        }

        /// <summary>
        /// Appends the path segment to this path, extending it.
        /// </summary>
        /// <param name="segment">The segment.</param>
        /// <returns>The union of the two this and the other path.</returns>
        public Path AppendSegment(Path segment)
        {
            var sourceCount = segment._used;
            if (sourceCount == 0)
            {
                return this;
            }

            //The most common scenario is that this is used when on the last leg of a path to extend it.
            //In that case just return the segment skipping the first node (since that is the end of this path already).
            if (_used == 0)
            {
                segment.Pop();
                return segment;
            }

            //This path has 2+ elements left so concat this with the other path
            var size = (_used + sourceCount) - 1;
            var newArr = new IPositioned[size];
            var source = segment._array;

            Array.Copy(source, newArr, sourceCount - 1);
            Array.Copy(_array, 0, newArr, sourceCount - 1, _used);

            _used = size;
            _array = newArr;
            return this;
        }

        /// <summary>
        /// Gets the concrete path points, i.e. the positions that make up the path. This returns only the actual points and filters away portal nodes that are not an actual part of the path.
        /// </summary>
        /// <returns>An enumerator for all points currently in the path.</returns>
        public IEnumerable<Vector3> GetPoints()
        {
            var limit = this.count;
            for (int i = 0; i < limit; i++)
            {
                if (this[i] is IPortalNode)
                {
                    continue;
                }

                yield return this[i].position;
            }
        }

        /// <summary>
        /// Calculates the squared length. This is the sum of each path segment squared, not the total length squared.
        /// </summary>
        /// <returns>The squared length</returns>
        public float CalculateSquaredLength()
        {
            float length = 0f;

            var lastIdx = this.count - 2;
            for (int i = 0; i <= lastIdx; i++)
            {
                //Seeing as a portal is never the first nor the last node, this is valid
                var first = this[i];
                var second = this[i + 1];
                if (second is IPortalNode)
                {
                    second = this[++i + 1];
                }

                length += (first.position - second.position).sqrMagnitude;
            }

            return length;
        }

        /// <summary>
        /// Calculates the length.
        /// </summary>
        /// <returns>The length of the path.</returns>
        public float CalculateLength()
        {
            float length = 0f;

            var lastIdx = this.count - 2;
            for (int i = 0; i <= lastIdx; i++)
            {
                //Seeing as a portal is never the first nor the last node, this is valid
                var first = this[i];
                var second = this[i + 1];
                if (second is IPortalNode)
                {
                    second = this[++i + 1];
                }

                length += (first.position - second.position).magnitude;
            }

            _length = length;
            return length;
        }

        /// <summary>
        /// Updates the path.
        /// </summary>
        /// <param name="path">The path points.</param>
        public void Update(params Vector3[] path)
        {
            Clear();

            for (int i = path.Length - 1; i >= 0; i--)
            {
                Push(path[i].AsPositioned());
            }
        }

        /// <summary>
        /// Updates the path.
        /// </summary>
        /// <param name="path">The path points.</param>
        public void Update(IIndexable<Vector3> path)
        {
            Clear();

            for (int i = path.count - 1; i >= 0; i--)
            {
                Push(path[i].AsPositioned());
            }

            _length = -1f;
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public Path Clone()
        {
            return new Path(this);
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }
    }
}
