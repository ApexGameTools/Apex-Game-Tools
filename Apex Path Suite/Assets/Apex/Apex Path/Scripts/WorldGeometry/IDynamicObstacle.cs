/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    using Apex.Common;

    /// <summary>
    /// Interface for dynamic obstacles
    /// </summary>
    public partial interface IDynamicObstacle : IPositioned
    {
        /// <summary>
        /// Gets the attribute mask that defines the attributes for which this obstacle will not be considered an obstacle.
        /// </summary>
        /// <value>
        /// The exceptions mask.
        /// </value>
        AttributeMask exceptionsMask { get; }

        /// <summary>
        /// Explicitly starts updating the dynamic obstacle, making it reevaluate its state.
        /// </summary>
        /// <param name="interval">The interval by which to update. Pass null to use the default interval defined by the load balancer.</param>
        /// <param name="repeat">if set to <c>true</c> it will repeatedly update every <paramref name="interval"/> otherwise it will update only once.</param>
        void ActivateUpdates(float? interval, bool repeat);

        /// <summary>
        /// Toggles the obstacle on or off. This is preferred to enabling/disabling the component if it is a regularly recurring action.
        /// </summary>
        /// <param name="active">if set to <c>true</c> the obstacle is toggle on, otherwise off.</param>
        void Toggle(bool active);
    }
}
