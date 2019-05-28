/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering.Behaviours
{
    using UnityEngine;

    /// <summary>
    /// A steering behaviour that will make the unit to which it is attached, wander around within a certain radius.
    /// </summary>
    [AddComponentMenu("Apex/Unit/Navigation/Wander", 1033)]
    [ApexComponent("Behaviours")]
    public class WanderComponent : ExtendedMonoBehaviour
    {
        /// <summary>
        /// The radius from the starting position within which to wander
        /// </summary>
        public float radius = 10.0f;

        /// <summary>
        /// The minimum distance of a wander route
        /// </summary>
        public float minimumDistance = 4.0f;

        /// <summary>
        /// The time in seconds that the unit will linger after each wander route before moving on.
        /// </summary>
        public float lingerForSeconds = 0.0f;

        /// <summary>
        /// Called on Start and OnEnable, but only one of the two, i.e. at startup it is only called once.
        /// </summary>
        protected override void OnStartAndEnable()
        {
            var unit = this.GetUnitFacade();
            if (unit == null)
            {
                Debug.LogError("WanderBehaviour requires a component that implements IMovable.");
            }

            unit.Wander(this.radius, this.minimumDistance, this.lingerForSeconds);
        }

        private void OnDisable()
        {
            var unit = this.GetUnitFacade();
            if (unit != null)
            {
                unit.StopWander();
            }
        }
    }
}
