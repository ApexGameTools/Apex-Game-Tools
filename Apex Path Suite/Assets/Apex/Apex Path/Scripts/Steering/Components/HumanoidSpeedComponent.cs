/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering.Components
{
    using UnityEngine;

    /// <summary>
    /// A component to define the speed settings for a humanoid unit.
    /// </summary>
    [AddComponentMenu("Apex/Unit/Navigation/Humanoid Speed", 1027)]
    [ApexComponent("Steering")]
    public class HumanoidSpeedComponent : SpeedComponent
    {
        /// <summary>
        /// The crawl speed
        /// </summary>
        public float crawlSpeed = 1.0f;

        /// <summary>
        /// The crouched speed
        /// </summary>
        public float crouchedSpeed = 1.5f;

        /// <summary>
        /// The walk speed
        /// </summary>
        public float walkSpeed = 3.0f;

        /// <summary>
        /// The run speed
        /// </summary>
        public float runSpeed = 6.0f;

        /// <summary>
        /// The strafe maximum speed, that is the highest speed possible when moving side ways
        /// </summary>
        public float strafeMaxSpeed = 4.0f;

        /// <summary>
        /// The back pedal maximum speed, that is the highest speed possible when moving backwards
        /// </summary>
        public float backPedalMaxSpeed = 3.0f;

        private Transform _transform;

        /// <summary>
        /// Gets the maximum speed of the unit.
        /// </summary>
        /// <value>
        /// The maximum speed.
        /// </value>
        public override float maximumSpeed
        {
            get { return this.runSpeed; }
            set { this.runSpeed = value; }
        }

        private void Awake()
        {
            this.WarnIfMultipleInstances();

            _transform = this.transform;
            Walk();
        }

        /// <summary>
        /// Makes the unit's preferred speed that of <see cref="crawlSpeed"/>
        /// </summary>
        public void Crawl()
        {
            _preferredSpeed = this.crawlSpeed;
        }

        /// <summary>
        /// Makes the unit's preferred speed that of <see cref="crouchedSpeed"/>
        /// </summary>
        public void Crouch()
        {
            _preferredSpeed = this.crouchedSpeed;
        }

        /// <summary>
        /// Makes the unit's preferred speed that of <see cref="walkSpeed"/>
        /// </summary>
        public void Walk()
        {
            _preferredSpeed = this.walkSpeed;
        }

        /// <summary>
        /// Makes the unit's preferred speed that of <see cref="runSpeed"/>
        /// </summary>
        public void Run()
        {
            _preferredSpeed = this.runSpeed;
        }

        /// <summary>
        /// Gets the preferred speed of the unit.
        /// </summary>
        /// <param name="currentMovementDirection">The current movement direction.</param>
        /// <returns>
        /// The preferred speed
        /// </returns>
        public override float GetPreferredSpeed(Vector3 currentMovementDirection)
        {
            var dp = Vector3.Dot(currentMovementDirection, _transform.forward);

            if (dp < 0.5f)
            {
                //Between 60 and 120 degrees movement is considered a strafe
                if ((_preferredSpeed >= this.strafeMaxSpeed) && (dp > -0.5f))
                {
                    return this.strafeMaxSpeed;
                }

                //Beyond 120 degrees its a backpedal.
                if (_preferredSpeed > this.backPedalMaxSpeed)
                {
                    return this.backPedalMaxSpeed;
                }
            }

            return _preferredSpeed;
        }

        /// <summary>
        /// Clones from the other component.
        /// </summary>
        /// <param name="speedComponent">The component to clone from</param>
        public override void CloneFrom(IDefineSpeed speedComponent)
        {
            base.CloneFrom(speedComponent);

            var hs = speedComponent as HumanoidSpeedComponent;
            if (hs != null)
            {
                this.crawlSpeed = hs.crawlSpeed;
                this.crouchedSpeed = hs.crouchedSpeed;
                this.walkSpeed = hs.walkSpeed;
                this.runSpeed = hs.runSpeed;
                this.strafeMaxSpeed = hs.strafeMaxSpeed;
                this.backPedalMaxSpeed = hs.backPedalMaxSpeed;
            }
        }
    }
}
