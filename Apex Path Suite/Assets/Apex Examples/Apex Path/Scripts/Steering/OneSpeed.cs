#pragma warning disable 1591
namespace Apex.Examples.Steering
{
    using Apex.Steering;
    using UnityEngine;

    /// <summary>
    /// An example speed controller that implements IDefine speed itself instead of deriving from SpeedComponent
    /// </summary>
    [AddComponentMenu("Apex/Examples/One Speed", 1014)]
    public class OneSpeed : MonoBehaviour, IDefineSpeed
    {
        /// <summary>
        /// The one and only speed
        /// </summary>
        public float speed;

        /// <summary>
        /// Gets the minimum speed of the unit.
        /// </summary>
        /// <value>
        /// The minimum speed.
        /// </value>
        public float minimumSpeed
        {
            get { return speed / 2.0f; }
        }

        /// <summary>
        /// Gets the maximum speed of the unit.
        /// </summary>
        /// <value>
        /// The maximum speed.
        /// </value>
        public float maximumSpeed
        {
            get { return speed; }
        }

        /// <summary>
        /// Gets the maximum acceleration.
        /// </summary>
        /// <value>
        /// The maximum acceleration.
        /// </value>
        public float maxAcceleration
        {
            get { return speed * 10f; }
        }

        /// <summary>
        /// Gets the maximum deceleration.
        /// </summary>
        /// <value>
        /// The maximum deceleration.
        /// </value>
        public float maxDeceleration
        {
            get { return speed * 15f; }
        }

        //This unit cannot turn at all
        public float maxAngularAcceleration
        {
            get { return 0f; }
        }

        //This unit cannot turn at all
        public float maximumAngularSpeed
        {
            get { return 0f; }
        }

        /// <summary>
        /// Signal that the unit has stopped.
        /// </summary>
        public void SignalStop()
        {
            /* NOOP */
        }

        /// <summary>
        /// Gets the preferred speed of the unit.
        /// </summary>
        /// <param name="currentMovementDirection">The current movement direction.</param>
        /// <returns>
        /// The preferred speed
        /// </returns>
        public float GetPreferredSpeed(Vector3 currentMovementDirection)
        {
            return speed;
        }

        public void SetPreferredSpeed(float speed)
        {
            /* We just ignore this, since this only has one fixed speed */
        }

        public void CloneFrom(IDefineSpeed speedComponent)
        {
            var c = speedComponent as OneSpeed;
            if (c != null)
            {
                this.speed = c.speed;
            }
        }
    }
}
