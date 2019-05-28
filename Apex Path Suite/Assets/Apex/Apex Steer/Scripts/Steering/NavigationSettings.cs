/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Steering
{
    /// <summary>
    /// A class responsible for publicly exposing any and all global navigation settings.
    /// </summary>
    public class NavigationSettings
    {
        /// <summary>
        /// The steering transient unit group load balanced update interval.
        /// </summary>
        public float groupUpdateInterval;

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationSettings"/> class.
        /// </summary>
        /// <param name="updateInterval">The steering transient unit group load balanced update interval.</param>
        public NavigationSettings(float updateInterval)
        {
            this.groupUpdateInterval = updateInterval;
        }
    }
}