/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering.Props
{
    using System;
    using UnityEngine;

    /// <summary>
    /// A component that represents a patrol route used by <see cref="Apex.Steering.Behaviours.PatrolBehaviour"/> or other components who find it useful.
    /// </summary>
    [AddComponentMenu("Apex/Legacy/Navigation/Patrol Route (OLD)", 1022)]
    public class PatrolRoute : MonoBehaviour
    {
        /// <summary>
        /// Whether or not to always draw patrol point gizmos, regardless of selected state.
        /// </summary>
        public bool drawGizmosAlways;

        /// <summary>
        /// The gizmo color
        /// </summary>
        public Color gizmoColor = new Color(0f, 150f / 255f, 211f / 255f);

        private PatrolPoint[] _patrolPoints;

        /// <summary>
        /// Gets the patrol points.
        /// </summary>
        /// <value>
        /// The patrol points.
        /// </value>
        public PatrolPoint[] patrolPoints
        {
            get { return _patrolPoints; }
        }

        private void Awake()
        {
            _patrolPoints = GetComponentsInChildren<PatrolPoint>();
            Array.Sort(
                _patrolPoints,
                (a, b) =>
                {
                    var c = a.orderIndex.CompareTo(b.orderIndex);
                    if (c == 0)
                    {
                        return a.gameObject.name.CompareTo(b.gameObject.name);
                    }

                    return c;
                });
        }
    }
}
