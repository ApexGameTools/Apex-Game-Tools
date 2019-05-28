/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering.Behaviours
{
    using UnityEngine;

    /// <summary>
    /// A steering behaviour that will make the unit to which it is attached, patrol a predefined route.
    /// </summary>
    [AddComponentMenu("Apex/Unit/Navigation/Patrol", 1029)]
    [ApexComponent("Behaviours")]
    public class PatrolComponent : ExtendedMonoBehaviour
    {
        /// <summary>
        /// If set to true, the patrol points are visited in a random manner rather than sequentially in order.
        /// </summary>
        public bool randomize = false;

        /// <summary>
        /// Whether to patrol the route in reverse direction.
        /// </summary>
        public bool reverse = false;

        /// <summary>
        /// The time in seconds that the unit will linger at each patrol point before moving on.
        /// </summary>
        public float lingerForSeconds = 0.0f;

        /// <summary>
        /// The patrol route
        /// </summary>
        public PatrolPointsComponent route;

        /// <summary>
        /// Called on Start and OnEnable, but only one of the two, i.e. at startup it is only called once.
        /// </summary>
        protected override void OnStartAndEnable()
        {
            var unit = this.GetUnitFacade();
            if (unit == null)
            {
                Debug.LogError("PatrolComponent requires a component that implements IMovable.");
            }

            if (route == null)
            {
                Debug.LogWarning("Unit has no patrol route set.");
                return;
            }

            unit.Patrol(route.worldPoints, this.randomize, this.reverse, this.lingerForSeconds);
        }

        private void OnDisable()
        {
            var unit = this.GetUnitFacade();
            if (unit != null)
            {
                unit.StopPatrol();
            }
        }
    }
}
