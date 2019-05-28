/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering.HeightNavigation
{
    /// <summary>
    /// Interface for unit height providers also referred to as height samplers.
    /// </summary>
    public interface IUnitHeightProvider
    {
        /// <summary>
        /// Gets the height delta, i.e. the difference in height between where the unit will be at the end of the frame and the height the unit should aim to be at..
        /// </summary>
        /// <param name="input">The steering input</param>
        /// <returns>The height delta</returns>
        float GetHeightDelta(SteeringInput input);
    }
}
