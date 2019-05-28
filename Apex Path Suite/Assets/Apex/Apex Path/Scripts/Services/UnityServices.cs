/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Services
{
    using UnityEngine;

    /// <summary>
    /// Exposes various Unity services in wrapped form enabling unit testing.
    /// </summary>
    public static class UnityServices
    {
        /// <summary>
        /// Exposes the functionality of Unity's Physics object through a wrapper
        /// </summary>
        public static IPhysics physics = new PhysicsWrapper();

        /// <summary>
        /// Exposes the functionality of Unity's LayerMask object through a wrapper
        /// </summary>
        public static ILayerMask layers = new LayerMaskWrapper();

        /// <summary>
        /// Exposes the functionality of Unity's Time object through a wrapper
        /// </summary>
        public static ITime time = new TimeWrapper();

        /// <summary>
        /// Exposes the functionality of Unity's Debug object through a wrapper
        /// </summary>
        public static IDebug debug = new DebugWrapper();

        /// <summary>
        /// Wraps Camera.main, issuing an error message if no main camera exists.
        /// </summary>
        public static Camera mainCamera
        {
            get
            {
                var cam = Camera.main;
                if (cam == null)
                {
                    throw new MissingReferenceException("There is no main camera defined, please tag one camera as main.");
                }

                return cam;
            }
        }
    }
}
