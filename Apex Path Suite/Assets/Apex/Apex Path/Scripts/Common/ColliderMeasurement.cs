/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Common
{
    using UnityEngine;

    /// <summary>
    /// Exposes measurements of a collider
    /// </summary>
    public class ColliderMeasurement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ColliderMeasurement"/> class.
        /// </summary>
        /// <param name="c">The collider to measure.</param>
        /// <param name="inWorldSpace">Whether values are in world space.</param>
        public ColliderMeasurement(Collider c, bool inWorldSpace)
        {
            if (c is CapsuleCollider)
            {
                var cc = c as CapsuleCollider;
                if (inWorldSpace)
                {
                    var t = cc.transform;
                    var scale = t.lossyScale;
                    this.center = t.TransformPoint(cc.center);
                    this.transformCenterOffset = this.center - t.position;

                    var diameter = 2f * cc.radius * Mathf.Max(scale.x, scale.z);
                    var length = Mathf.Max(cc.height * scale.y, diameter);
                    switch (cc.direction)
                    {
                        //x-axis
                        case 0:
                        {
                            this.size = new Vector3(length, diameter, diameter);
                            break;
                        }

                        //y-axis
                        case 1:
                        {
                            this.size = new Vector3(diameter, length, diameter);
                            break;
                        }

                        //z-axis
                        case 2:
                        {
                            this.size = new Vector3(diameter, diameter, length);
                            break;
                        }
                    }
                }
                else
                {
                    this.center = cc.center;
                    this.transformCenterOffset = this.center;

                    var diameter = cc.radius * 2f;
                    var length = Mathf.Max(cc.height, diameter);
                    switch (cc.direction)
                    {
                        //x-axis
                        case 0:
                        {
                            this.size = new Vector3(length, diameter, diameter);
                            break;
                        }

                        //y-axis
                        case 1:
                        {
                            this.size = new Vector3(diameter, length, diameter);
                            break;
                        }

                        //z-axis
                        case 2:
                        {
                            this.size = new Vector3(diameter, diameter, length);
                            break;
                        }
                    }
                }
            }
            else if (c is SphereCollider)
            {
                var sc = c as SphereCollider;

                if (inWorldSpace)
                {
                    var t = sc.transform;
                    var scale = t.lossyScale;
                    var diameter = 2f * sc.radius * Mathf.Max(scale.x, scale.y, scale.z);
                    this.size = new Vector3(diameter, diameter, diameter);
                    this.center = t.TransformPoint(sc.center);
                    this.transformCenterOffset = this.center - t.position;
                }
                else
                {
                    var diameter = 2f * sc.radius;
                    this.size = new Vector3(diameter, diameter, diameter);
                    this.center = sc.center;
                    this.transformCenterOffset = this.center;
                }
            }
            else if (c is BoxCollider)
            {
                var bc = c as BoxCollider;

                if (inWorldSpace)
                {
                    var t = bc.transform;
                    var scale = t.lossyScale;
                    this.size = new Vector3(bc.size.x * scale.x, bc.size.y * scale.y, bc.size.z * scale.z);
                    this.center = t.TransformPoint(bc.center);
                    this.transformCenterOffset = this.center - t.position;
                }
                else
                {
                    this.size = bc.size;
                    this.center = bc.center;
                    this.transformCenterOffset = this.center;
                }
            }
            else if (c is MeshCollider)
            {
                var mc = c as MeshCollider;
                var t = mc.transform;

                var curRotation = t.rotation;
                t.rotation = Quaternion.identity;

                if (inWorldSpace)
                {
                    this.size = mc.bounds.size;
                    this.center = mc.bounds.center;
                    this.transformCenterOffset = this.center - t.position;
                }
                else
                {
                    var curScale = t.lossyScale;
                    var bs = mc.bounds.size;
                    this.size = new Vector3(bs.x / curScale.x, bs.y / curScale.y, bs.z / curScale.z);
                    this.center = t.InverseTransformPoint(mc.bounds.center);
                    this.transformCenterOffset = this.center;
                }

                t.rotation = curRotation;
            }

            this.extents = this.size * 0.5f;
        }

        /// <summary>
        /// Gets the size.
        /// </summary>
        /// <value>
        /// The size.
        /// </value>
        public Vector3 size
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the extents.
        /// </summary>
        /// <value>
        /// The extents.
        /// </value>
        public Vector3 extents
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the center.
        /// </summary>
        /// <value>
        /// The center.
        /// </value>
        public Vector3 center
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the offset between the collider center and the transform center, i.e. the transform center plus this will give <see cref="center"/>. For local space this is equal to <see cref="center"/>.
        /// </summary>
        /// <value>
        /// The transform center offset.
        /// </value>
        public Vector3 transformCenterOffset
        {
            get;
            private set;
        }
    }
}
