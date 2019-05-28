/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Steering
{
    using Apex.Services;
    using Apex.Utilities;

    /// <summary>
    /// A partial NavigationSettingsComponent specific to Apex Steer.
    /// Adds the steering transient unit group update interval for public exposure
    /// </summary>
    public partial class NavigationSettingsComponent
    {
        /// <summary>
        /// The steering transient unit group load balanced update interval.
        /// </summary>
        [MinCheck(0.01f, label = "Group Update Interval", tooltip = "The interval used for the load balanced updating of the default transient steering group")]
        public float groupUpdateInterval = 0.1f;

        partial void RefreshPartial()
        {
            GameServices.navigationSettings = new NavigationSettings(groupUpdateInterval);
        }
    }
}