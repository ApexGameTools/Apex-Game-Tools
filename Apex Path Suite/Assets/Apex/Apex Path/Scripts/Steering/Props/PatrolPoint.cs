/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering.Props
{
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// A component that represents a point on a <see cref="PatrolRoute"/>
    /// </summary>
    [AddComponentMenu("Apex/Legacy/Navigation/Patrol Point (OLD)", 1021)]
    public class PatrolPoint : MonoBehaviour, IPositioned
    {
        /// <summary>
        /// The order index used to order this point in relation to other points
        /// </summary>
        public int orderIndex;

        /// <summary>
        /// Whether to use the GameObject's Transform as the patrol point position. If set to <c>false</c> <see cref="location"/> is used instead.
        /// </summary>
        public bool useTransformPosition = true;

        /// <summary>
        /// The location of this patrol point if <see cref="useTransformPosition"/> is set to <c>false</c>
        /// </summary>
        public Vector3 location;

#if UNITY_EDITOR
        private PatrolRoute _parent;
#endif

        /// <summary>
        /// Gets the position of the patrol point.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        public Vector3 position
        {
            get
            {
                if (this.useTransformPosition)
                {
                    return this.transform.position;
                }

                return this.location;
            }
        }

        private void OnDrawGizmos()
        {
            DrawGizmos(false);
        }

        private void OnDrawGizmosSelected()
        {
            DrawGizmos(true);
        }

        private void DrawGizmos(bool selected)
        {
            if (!this.enabled)
            {
                return;
            }

            Color c = Color.yellow;

#if UNITY_EDITOR
            if (_parent == null && transform.parent != null)
            {
                _parent = transform.parent.GetComponent<PatrolRoute>();
            }

            if (_parent != null)
            {
                c = _parent.gizmoColor;
                selected = selected || _parent.drawGizmosAlways;
            }
#endif

            if (selected)
            {
                Gizmos.color = c;
                Gizmos.DrawSphere(this.position, 0.2f);
            }
        }
    }
}
