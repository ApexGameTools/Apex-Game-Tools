/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    using System.Collections.Generic;
    using Apex.DataStructures;
    using Apex.Utilities;
    using UnityEngine;

    /// <summary>
    /// Represents an obstacle with a dynamic nature, meaning it can be an obstacle to only some, only at certain times, etc.
    /// </summary>
    public partial class DynamicObstacle : ISupportRuntimeStateChange
    {
        /// <summary>
        /// Controls how the dynamic obstacle determines which cells it covers.
        /// </summary>
        public Bounding boundingMode = Bounding.AxisAligned;

        /// <summary>
        /// The adaption rotation threshold controls how much rotation around the x- and/or z-axis is required before switching to full generic mode, which is more precise but also more costly.
        /// </summary>
        [Range(1f, 90f)]
        public float adaptationRotationThreshold = 10f;

        /// <summary>
        /// Defines the options for the bounding mode of the obstacle
        /// </summary>
        public enum Bounding
        {
            /// <summary>
            /// Axis aligned bounding, i.e. AABB. Suboptimal if the obstacle itself is not axis aligned.
            /// </summary>
            AxisAligned,

            /// <summary>
            /// Adapts the bounding to the best approximation and the best performance possible according to rotation.
            /// </summary>
            Adaptive,

            /// <summary>
            /// Only calculates the obstacle outline once. Requires no velocity and that no rotation is done on the x- and z- axis after the obstacle has initialized.
            /// </summary>
            FixedHull
        }

        internal float adaptationChangeThreshold
        {
            get;
            set;
        }

        /// <summary>
        /// Called on awake.
        /// </summary>
        partial void ExtensionInit()
        {
            if (this.boundingMode == Bounding.Adaptive || this.boundingMode == Bounding.FixedHull)
            {
                var coll = this.GetComponent<Collider>();
                if (coll is BoxCollider)
                {
                    _actualBounds = new BoxColliderBounds(coll as BoxCollider, this);
                }
                else if (coll is SphereCollider)
                {
                    _actualBounds = new SphereColliderBounds(coll as SphereCollider, this);
                }
                else if (coll is CapsuleCollider)
                {
                    _actualBounds = new CapsuleColliderBounds(coll as CapsuleCollider, this);
                }
                else if (coll is MeshCollider)
                {
                    _actualBounds = new MeshColliderBounds(coll as MeshCollider, this);
                }
                else
                {
                    Debug.LogError(string.Format("The collider on the Dynamic Obstacle on game object '{0}' does not support Adaptive bounding. Mode has been changed to AxisAligned.", gameObject.name));
                }
            }

            this.adaptationChangeThreshold = Mathf.Cos(this.adaptationRotationThreshold * Mathf.Deg2Rad);
        }

        void ISupportRuntimeStateChange.ReevaluateState()
        {
            this.adaptationChangeThreshold = Mathf.Cos(this.adaptationRotationThreshold * Mathf.Deg2Rad);

            var hull = _actualBounds as ISupportRuntimeStateChange;
            if (hull != null)
            {
                hull.ReevaluateState();
            }
        }

        /// <summary>
        /// Refreshes the hull of the dynamic obstacle. Only relevant for MeshCollider obstacles and should only be called if the mesh changes at runtime (actual changes, not transform scaling or rotation).
        /// </summary>
        public override void RefreshHull()
        {
            var hull = _actualBounds as MeshColliderBounds;
            if (hull != null)
            {
                hull.RefreshHull(this.GetComponent<MeshCollider>());
            }
        }

        private sealed class BoxColliderBounds : PrimitiveColliderBounds
        {
            private BoxCollider _collider;

            public BoxColliderBounds(BoxCollider collider, DynamicObstacle parent)
                : base(collider.transform, parent)
            {
                _collider = collider;
            }

            protected override Vector3 center
            {
                get { return _collider.center; }
            }

            protected override float extentsX
            {
                get { return _collider.size.x / 2f; }
            }

            protected override float extentsY
            {
                get { return _collider.size.y / 2f; }
            }

            protected override float extentsZ
            {
                get { return _collider.size.z / 2f; }
            }
        }

        private sealed class SphereColliderBounds : PrimitiveColliderBounds
        {
            private SphereCollider _collider;

            public SphereColliderBounds(SphereCollider collider, DynamicObstacle parent)
                : base(collider.transform, parent)
            {
                _collider = collider;
            }

            protected override Vector3 center
            {
                get { return _collider.center; }
            }

            protected override float extentsX
            {
                get { return _collider.radius; }
            }

            protected override float extentsY
            {
                get { return _collider.radius; }
            }

            protected override float extentsZ
            {
                get { return _collider.radius; }
            }
        }

        private sealed class CapsuleColliderBounds : PrimitiveColliderBounds
        {
            private CapsuleCollider _collider;

            public CapsuleColliderBounds(CapsuleCollider collider, DynamicObstacle parent)
                : base(collider.transform, parent)
            {
                _collider = collider;
            }

            protected override Vector3 center
            {
                get { return _collider.center; }
            }

            protected override float extentsX
            {
                get { return _collider.direction == 0 ? _collider.height : _collider.radius; }
            }

            protected override float extentsY
            {
                get { return _collider.direction == 1 ? _collider.height : _collider.radius; }
            }

            protected override float extentsZ
            {
                get { return _collider.direction == 2 ? _collider.height : _collider.radius; }
            }
        }

        private abstract class ColliderBoundsBase : IActualBounds
        {
            protected readonly Transform _transform;
            protected readonly DynamicObstacle _parent;
            protected ConvexHull _actualBounds;
            protected ConvexHull _oldBounds;
            private MatrixBounds _cellCoverage;

            public ColliderBoundsBase(Transform t, DynamicObstacle parent)
            {
                _transform = t;
                _parent = parent;
                _cellCoverage = MatrixBounds.nullBounds;
            }

            public MatrixBounds Prepare(CellMatrix matrix, bool block)
            {
                MatrixBounds combinedCoverage;

                //The bounds used last check
                _oldBounds.Mirror(_actualBounds);

                if (!block)
                {
                    //Only unblock is required, so the affected area is the most recent coverage
                    combinedCoverage = _cellCoverage;
                    _actualBounds.Reset();
                    _cellCoverage = MatrixBounds.nullBounds;

                    return combinedCoverage;
                }

                var velocity = _parent.GetVelocity();

                var obstacleSensitivityRange = _parent.useGridObstacleSensitivity ? matrix.obstacleSensitivityRange : _parent.customSensitivity;

                PrepareHull(velocity, obstacleSensitivityRange);

                //Get the axis-aligned bounding box
                var bounds = _actualBounds.CalculateBounds();

                //Get the new coverage, combine it with the old for coverage of both those to unblock and block 
                var newCoverage = matrix.GetMatrixBounds(bounds, matrix.cellSize / 2f, true);
                combinedCoverage = MatrixBounds.Combine(_cellCoverage, newCoverage);
                _cellCoverage = newCoverage;

                return combinedCoverage;
            }

            public bool Update(Cell c)
            {
                bool contains = _actualBounds.Contains(c.position);
                bool contained = _oldBounds.Contains(c.position);

                if (contained && !contains)
                {
                    return c.RemoveDynamicObstacle(_parent);
                }
                else if (contains && !contained)
                {
                    return c.AddDynamicObstacle(_parent);
                }

                return false;
            }

            public void Render()
            {
                float ypos = _transform.position.y;
                int count = _actualBounds.hullPointsCount;
                for (int i = 0; i < count; i++)
                {
                    Gizmos.DrawLine(_actualBounds[i].AdjustAxis(ypos, Axis.Y), _actualBounds[(i + 1) % count].AdjustAxis(ypos, Axis.Y));
                }
            }

            protected abstract void PrepareHull(Vector3 velocity, float obstacleSensitivityRange);
        }

        private abstract class PrimitiveColliderBounds : ColliderBoundsBase
        {
            public PrimitiveColliderBounds(Transform t, DynamicObstacle parent)
                : base(t, parent)
            {
                _actualBounds = new ConvexHull(8, parent.isVelocityEnabled);
                _oldBounds = new ConvexHull(8, parent.isVelocityEnabled);
            }

            protected abstract Vector3 center { get; }

            protected abstract float extentsX { get; }

            protected abstract float extentsY { get; }

            protected abstract float extentsZ { get; }

            protected override void PrepareHull(Vector3 velocity, float obstacleSensitivityRange)
            {
                //Use full generic mode when x and/or z rotation exceeds the threshold
                var rotationOffset = Mathf.Abs(Vector3.Dot(_transform.up, Vector3.up));
                if (rotationOffset < _parent.adaptationChangeThreshold)
                {
                    PrepareForGeneric(velocity, obstacleSensitivityRange);
                }
                else
                {
                    PrepareForFixedXZ(velocity, obstacleSensitivityRange);
                }
            }

            private void PrepareForGeneric(Vector3 velocity, float obstacleSensitivityRange)
            {
                //Calculate the offsets from the collider center to get the corners of the actual bounding box
                var scale = _transform.lossyScale;

                var dx = this.extentsX + (obstacleSensitivityRange / scale.x);
                var dz = this.extentsZ + (obstacleSensitivityRange / scale.z);
                var dy = this.extentsY + (obstacleSensitivityRange / scale.y);
                var center = this.center;

                //Create the bounding hull
                _actualBounds[0] = _transform.TransformPoint(center.x - dx, center.y + dy, center.z + dz);
                _actualBounds[1] = _transform.TransformPoint(center.x - dx, center.y + dy, center.z - dz);
                _actualBounds[2] = _transform.TransformPoint(center.x + dx, center.y + dy, center.z - dz);
                _actualBounds[3] = _transform.TransformPoint(center.x + dx, center.y + dy, center.z + dz);
                _actualBounds[4] = _transform.TransformPoint(center.x - dx, center.y - dy, center.z + dz);
                _actualBounds[5] = _transform.TransformPoint(center.x - dx, center.y - dy, center.z - dz);
                _actualBounds[6] = _transform.TransformPoint(center.x + dx, center.y - dy, center.z - dz);
                _actualBounds[7] = _transform.TransformPoint(center.x + dx, center.y - dy, center.z + dz);

                if (velocity.sqrMagnitude > 0f)
                {
                    _actualBounds.CalculateHull(velocity);
                }
                else
                {
                    _actualBounds.CalculateHull();
                }
            }

            private void PrepareForFixedXZ(Vector3 velocity, float obstacleSensitivityRange)
            {
                //Calculate the offsets from the collider center to get the corners of the actual bounding rectangle
                var scale = _transform.lossyScale;

                var dx = this.extentsX + (obstacleSensitivityRange / scale.x);
                var dz = this.extentsZ + (obstacleSensitivityRange / scale.z);
                var center = this.center;

                //Adjust to velocity
                if (velocity.sqrMagnitude == 0f)
                {
                    //Create the bounding hull
                    _actualBounds[0] = _transform.TransformPoint(center.x - dx, center.y, center.z + dz);
                    _actualBounds[1] = _transform.TransformPoint(center.x - dx, center.y, center.z - dz);
                    _actualBounds[2] = _transform.TransformPoint(center.x + dx, center.y, center.z - dz);
                    _actualBounds[3] = _transform.TransformPoint(center.x + dx, center.y, center.z + dz);
                    _actualBounds.hullPointsCount = 4;
                }
                else
                {
                    velocity = _transform.InverseTransformPoint(_transform.position + velocity);

                    var topLeft = _transform.TransformPoint(center.x - dx, center.y, center.z + dz);
                    var bottomLeft = _transform.TransformPoint(center.x - dx, center.y, center.z - dz);
                    var topRight = _transform.TransformPoint(center.x + dx, center.y, center.z + dz);
                    var bottomRight = _transform.TransformPoint(center.x + dx, center.y, center.z - dz);

                    center = center + velocity;

                    var topLeftDisplaced = _transform.TransformPoint(center.x - dx, center.y, center.z + dz);
                    var bottomLeftDisplaced = _transform.TransformPoint(center.x - dx, center.y, center.z - dz);
                    var topRightDisplaced = _transform.TransformPoint(center.x + dx, center.y, center.z + dz);
                    var bottomRightDisplaced = _transform.TransformPoint(center.x + dx, center.y, center.z - dz);

                    if (velocity.x > 0f)
                    {
                        _actualBounds[0] = topLeft;
                        _actualBounds[1] = bottomLeft;

                        if (velocity.z > 0f)
                        {
                            _actualBounds[2] = bottomRight;
                            _actualBounds[3] = bottomRightDisplaced;
                            _actualBounds[4] = topRightDisplaced;
                            _actualBounds[5] = topLeftDisplaced;
                            _actualBounds.hullPointsCount = 6;
                        }
                        else if (velocity.z < 0f)
                        {
                            _actualBounds[2] = bottomLeftDisplaced;
                            _actualBounds[3] = bottomRightDisplaced;
                            _actualBounds[4] = topRightDisplaced;
                            _actualBounds[5] = topRight;
                            _actualBounds.hullPointsCount = 6;
                        }
                        else
                        {
                            _actualBounds[2] = bottomRightDisplaced;
                            _actualBounds[3] = topRightDisplaced;
                            _actualBounds.hullPointsCount = 4;
                        }
                    }
                    else if (velocity.x < 0f)
                    {
                        _actualBounds[0] = topRight;
                        _actualBounds[1] = bottomRight;

                        if (velocity.z > 0f)
                        {
                            _actualBounds[2] = bottomLeft;
                            _actualBounds[3] = bottomLeftDisplaced;
                            _actualBounds[4] = topLeftDisplaced;
                            _actualBounds[5] = topRightDisplaced;
                            _actualBounds.hullPointsCount = 6;
                        }
                        else if (velocity.z < 0f)
                        {
                            _actualBounds[2] = bottomRightDisplaced;
                            _actualBounds[3] = bottomLeftDisplaced;
                            _actualBounds[4] = topLeftDisplaced;
                            _actualBounds[5] = topLeft;
                            _actualBounds.hullPointsCount = 6;
                        }
                        else
                        {
                            _actualBounds[2] = bottomRightDisplaced;
                            _actualBounds[3] = topRightDisplaced;
                            _actualBounds.hullPointsCount = 4;
                        }
                    }
                    else if (velocity.z > 0f)
                    {
                        _actualBounds[0] = bottomLeft;
                        _actualBounds[1] = bottomRight;
                        _actualBounds[2] = topRightDisplaced;
                        _actualBounds[3] = topLeftDisplaced;
                        _actualBounds.hullPointsCount = 4;
                    }
                    else if (velocity.z < 0f)
                    {
                        _actualBounds[0] = topLeft;
                        _actualBounds[1] = topRight;
                        _actualBounds[2] = bottomRightDisplaced;
                        _actualBounds[3] = bottomLeftDisplaced;
                        _actualBounds.hullPointsCount = 4;
                    }
                }
            }
        }

        private sealed class MeshColliderBounds : ColliderBoundsBase, ISupportRuntimeStateChange
        {
            private const float _verticeClosenessThreshold = 0.0001f;

            private Vector3[] _vertices;
            private Vector3[] _paddingBuffer;
            private Bounding _boundingMode;

            public MeshColliderBounds(MeshCollider collider, DynamicObstacle parent)
                : base(collider.transform, parent)
            {
                RefreshHull(collider);
            }

            internal void RefreshHull(MeshCollider collider)
            {
                _boundingMode = _parent.boundingMode;
                var unique = new HashSet<Vector3>();

                var vertices = collider.sharedMesh.vertices;
                for (int i = 0; i < vertices.Length; i++)
                {
                    if (!unique.Contains(vertices[i]))
                    {
                        unique.Add(vertices[i]);
                    }
                }

                bool velocityEnabled = _parent.isVelocityEnabled;

                if (_boundingMode == Bounding.FixedHull)
                {
                    velocityEnabled = false;

                    //Create the temporary hull to get the hull vertices
                    var tmpHull = new ConvexHull(unique.Count, false);

                    int idx = 0;
                    foreach (var v in unique)
                    {
                        tmpHull[idx++] = v;
                    }

                    tmpHull.CalculateHull();

                    //Filter away vertices very close to each other
                    unique = new HashSet<Vector3>(new Vector3EqualityComparer(_verticeClosenessThreshold));
                    for (int i = 0; i < tmpHull.hullPointsCount; i++)
                    {
                        if (!unique.Contains(tmpHull[i]))
                        {
                            unique.Add(tmpHull[i]);
                        }
                    }
                }

                //Create the actual hulls etc.
                _vertices = new Vector3[unique.Count];
                unique.CopyTo(_vertices);

                EnsurePaddingBuffer();

                _actualBounds = new ConvexHull(_vertices.Length, velocityEnabled);
                _oldBounds = new ConvexHull(_vertices.Length, velocityEnabled);

                if (_boundingMode == Bounding.FixedHull)
                {
                    _actualBounds.hullPointsCount = _vertices.Length;
                }
            }

            void ISupportRuntimeStateChange.ReevaluateState()
            {
                EnsurePaddingBuffer();

                if (_boundingMode != _parent.boundingMode)
                {
                    RefreshHull(_parent.GetComponent<MeshCollider>());
                }
            }

            protected override void PrepareHull(Vector3 velocity, float obstacleSensitivityRange)
            {
                for (int i = 0; i < _vertices.Length; i++)
                {
                    _actualBounds[i] = _transform.TransformPoint(_vertices[i]);
                }

                if (_boundingMode == Bounding.Adaptive)
                {
                    if (velocity.sqrMagnitude > 0f)
                    {
                        _actualBounds.CalculateHull(velocity);
                    }
                    else
                    {
                        _actualBounds.CalculateHull();
                    }
                }

                if (obstacleSensitivityRange > 0f)
                {
                    AddPadding(obstacleSensitivityRange);
                }
            }

            private static Vector3 GetOutNormal(Vector3 p1, Vector3 p2)
            {
                var dx = p2.x - p1.x;
                var dz = p2.z - p1.z;
                var len = Mathf.Sqrt((dx * dx) + (dz * dz));

                return new Vector3(dz / len, 0f, -dx / len);
            }

            //It is assumed that vectors represent two edges in counter clockwise order
            private static Vector3 LineIntersection(Vector3 edgeAv1, Vector3 edgeAv2, Vector3 edgeBv1, Vector3 edgeBv2)
            {
                var denominator = ((edgeAv1.z - edgeAv2.z) * (edgeBv1.x - edgeBv2.x)) - ((edgeAv1.x - edgeAv2.x) * (edgeBv1.z - edgeBv2.z));

                if (denominator == 0f)
                {
                    //Line are parallel, or coincident.First of all this should not happen since the convex hull removes such cases
                    //But in case it does we just return the first of the two closest point of the two lines, i.e. the second vertice of the first edge.
                    return edgeBv1;
                }

                var ua = (((edgeAv1.x - edgeAv2.x) * (edgeBv2.z - edgeAv2.z)) - ((edgeAv1.z - edgeAv2.z) * (edgeBv2.x - edgeAv2.x))) / denominator;

                return new Vector3(edgeBv2.x + (ua * (edgeBv1.x - edgeBv2.x)), 0f, edgeBv2.z + (ua * (edgeBv1.z - edgeBv2.z)));
            }

            private void EnsurePaddingBuffer()
            {
                if (_parent.useGridObstacleSensitivity || _parent.customSensitivity > 0f)
                {
                    _paddingBuffer = new Vector3[_vertices.Length];
                }
            }

            private void AddPadding(float padding)
            {
                int count = _actualBounds.hullPointsCount;

                for (int i = 0; i < count; i++)
                {
                    var p1 = _actualBounds[i];
                    var p2 = _actualBounds[(i + 1) % count];
                    var p3 = _actualBounds[(i + 2) % count];

                    var normA = GetOutNormal(p1, p2);
                    var edgeAv1 = p1 + (normA * padding);
                    var edgeAv2 = p2 + (normA * padding);

                    var normB = GetOutNormal(p2, p3);
                    var edgeBv1 = p2 + (normB * padding);
                    var edgeBv2 = p3 + (normB * padding);

                    _paddingBuffer[(i + 1) % count] = LineIntersection(edgeAv1, edgeAv2, edgeBv1, edgeBv2);
                }

                for (int i = 0; i < count; i++)
                {
                    _actualBounds[i] = _paddingBuffer[i];
                }
            }
        }
    }
}
