/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering.Behaviours
{
    using UnityEngine;

    /// <summary>
    /// Component for defining patrol points using an editor.
    /// </summary>
    [AddComponentMenu("")]
    public class PatrolPointsComponent : MonoBehaviour
    {
        /// <summary>
        /// The way point color
        /// </summary>
        public Color pointColor = new Color(0f, 150f / 255f, 150f / 255f);

        /// <summary>
        /// The text color
        /// </summary>
        public Color textColor = Color.white;

        /// <summary>
        /// Controls whether the patrol points are seen as relative to their parent transform
        /// </summary>
        [Tooltip("Controls whether the patrol points are seen as relative to their parent transform. If relative they will move with the transform otherwise they will be static and remain where they were placed initially. It is recommended only to have this set to true when moving the parent and then set it back to false.")]
        public bool relativeToTransform;

        /// <summary>
        /// The waypoints that make up the patrol route, in either local or world coords depending on <see cref="relativeToTransform"/>.
        /// </summary>
        [Tooltip("The waypoints that make up the patrol route.")]
        public Vector3[] points = new Vector3[0];

        /// <summary>
        /// The waypoints that make up the patrol route in world coordinates.
        /// </summary>
        public Vector3[] worldPoints
        {
            get
            {
                if (relativeToTransform)
                {
                    var tpos = this.transform.position;
                    var worldPoints = new Vector3[points.Length];
                    for (int i = 0; i < worldPoints.Length; i++)
                    {
                        worldPoints[i] = points[i] + tpos;
                    }

                    return worldPoints;
                }
                else
                {
                    return points;
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = this.pointColor;
            foreach (var wp in this.worldPoints)
            {
                var pinHead = wp;
                pinHead.y += 1f;

                Gizmos.DrawLine(wp, pinHead);
                Gizmos.DrawSphere(pinHead, 0.3f);
            }
        }
    }
}
