/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering.HeightNavigation
{
    using UnityEngine;

    /// <summary>
    /// Represents output from the height navigation system
    /// </summary>
    public struct HeightOutput
    {
        /// <summary>
        /// The final velocity
        /// </summary>
        public Vector3 finalVelocity;

        /// <summary>
        /// Whether the unit is grounded
        /// </summary>
        public bool isGrounded;
    }
}
