/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Steering.Components
{
    using UnityEngine;

    /// <summary>
    /// This SteeringController components is responsible for managing solo path requests from steer for vector field component.
    /// </summary>
    [AddComponentMenu("Apex/Unit/Navigation/Steering/Steering Controller", 1037)]
    [ApexComponent("Steering")]
    public class SteeringController : ExtendedMonoBehaviour
    {
        private SteerForFormationComponent _steerForFormation;
        private SteerForPathComponent _steerForPath;

        /// <summary>
        /// Called on Start
        /// </summary>
        protected override void Start()
        {
            base.Start();

            _steerForFormation = this.GetComponent<SteerForFormationComponent>();
            _steerForPath = this.GetComponent<SteerForPathComponent>();
        }

        /// <summary>
        /// Starts the solo pathing - i.e. disables SteerForFormation
        /// </summary>
        public void StartSoloPath()
        {
            if (_steerForFormation != null)
            {
                _steerForFormation.enabled = false;
            }
        }

        /// <summary>
        /// Ends the solo pathing - i.e. enables SteerForFormation and disables SteerForPathComponent
        /// </summary>
        public void EndSoloPath()
        {
            if (_steerForFormation != null)
            {
                _steerForFormation.enabled = true;
            }

            if (_steerForPath != null)
            {
                _steerForPath.Stop();
            }
        }
    }
}