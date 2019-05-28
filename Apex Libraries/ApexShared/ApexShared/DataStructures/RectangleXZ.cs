/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.DataStructures
{
    using UnityEngine;

    /// <summary>
    /// Represents an axis aligned Rectangle on the xz plane
    /// </summary>
    public struct RectangleXZ
    {
        private float _minX;
        private float _minZ;
        private float _maxX;
        private float _maxZ;

        /// <summary>
        /// Initializes a new instance of the <see cref="RectangleXZ"/> class.
        /// </summary>
        /// <param name="center">The center.</param>
        /// <param name="width">The width.</param>
        /// <param name="depth">The depth.</param>
        public RectangleXZ(Vector3 center, float width, float depth)
        {
            _minX = center.x - (width / 2.0f);
            _minZ = center.z - (depth / 2.0f);

            _maxX = center.x + (width / 2.0f);
            _maxZ = center.z + (depth / 2.0f);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RectangleXZ"/> class.
        /// </summary>
        /// <param name="minX">The minimum x.</param>
        /// <param name="minZ">The minimum z.</param>
        /// <param name="width">The width.</param>
        /// <param name="depth">The depth.</param>
        public RectangleXZ(float minX, float minZ, float width, float depth)
        {
            _minX = minX;
            _minZ = minZ;

            _maxX = _minX + width;
            _maxZ = _minZ + depth;
        }

        /// <summary>
        /// Gets the width.
        /// </summary>
        /// <value>
        /// The width.
        /// </value>
        public float width
        {
            get { return _maxX - _minX; }
        }

        /// <summary>
        /// Gets the depth.
        /// </summary>
        /// <value>
        /// The depth.
        /// </value>
        public float depth
        {
            get { return _maxZ - _minZ; }
        }

        /// <summary>
        /// Gets the center of the rectangle.
        /// </summary>
        /// <value>
        /// The center.
        /// </value>
        public Vector3 center
        {
            get { return new Vector3(_minX + ((_maxX - _minX) / 2.0f), 0.0f, _minZ + ((_maxZ - _minZ) / 2.0f)); }
        }

        /// <summary>
        /// Gets the size as a vector.
        /// </summary>
        /// <value>
        /// The size.
        /// </value>
        public Vector3 size
        {
            get { return new Vector3(this.width, 0.0f, this.depth); }
        }

        /// <summary>
        /// Creates a rect from min/max values.
        /// </summary>
        /// <param name="minX">The minimum x.</param>
        /// <param name="minZ">The minimum z.</param>
        /// <param name="maxX">The maximum x.</param>
        /// <param name="maxZ">The maximum z.</param>
        public static RectangleXZ MinMaxRect(float minX, float minZ, float maxX, float maxZ)
        {
            return new RectangleXZ
            {
                _minX = minX,
                _minZ = minZ,

                _maxX = maxX,
                _maxZ = maxZ
            };
        }

        /// <summary>
        /// Determines whether <paramref name="point"/> is contained inside this rectangle.
        /// </summary>
        /// <param name="point">The point to check.</param>
        /// <returns><c>true</c> if contained, otherwise false.</returns>
        public bool Contains(Vector3 point)
        {
            if ((point.x < _minX) || (point.x > _maxX))
            {
                return false;
            }

            return ((point.z >= _minZ) && (point.z <= _maxZ));
        }

        /// <summary>
        /// Determines whether an<paramref name="other"/> rectangle is contained inside this rectangle.
        /// </summary>
        /// <param name="other">The rectangle to check.</param>
        /// <returns><c>true</c> if contained or identical, otherwise false.</returns>
        public bool Contains(RectangleXZ other)
        {
            return ((other._maxZ <= _maxZ) && (other._minZ >= _minZ) && (other._maxX <= _maxX) && (other._minX >= _minX));
        }

        /// <summary>
        /// Determines whether another rectangle overlaps this one (and vice versa).
        /// </summary>
        /// <param name="other">The other rectangle.</param>
        /// <returns><c>true</c> if they overlap, otherwise false.</returns>
        public bool Overlaps(RectangleXZ other)
        {
            if ((other._maxX <= _minX) || (other._minX >= _maxX))
            {
                return false;
            }

            return ((other._maxZ > _minZ) && (other._minZ < _maxZ));
        }

        /// <summary>
        /// Determines whether another bounds overlaps this one (and vice versa).
        /// </summary>
        /// <param name="b">The other bounds.</param>
        /// <returns><c>true</c> if they overlap, otherwise false.</returns>
        public bool Overlaps(Bounds b)
        {
            if ((b.max.x <= _minX) || (b.min.x >= _maxX))
            {
                return false;
            }

            return ((b.max.z > _minZ) && (b.min.z < _maxZ));
        }
    }
}
