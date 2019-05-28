/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering.HeightNavigation
{
    /// <summary>
    /// A height provider for flat maps.
    /// </summary>
    public class ZeroHeightProvider : IUnitHeightProvider
    {
        /// <summary>
        /// Gets the height delta, i.e. the difference in height between where the unit will be at the end of the frame and the height the unit should aim to be at..
        /// </summary>
        /// <param name="input">The steering input</param>
        /// <returns>
        /// The height delta
        /// </returns>
        public float GetHeightDelta(SteeringInput input)
        {
            var grid = input.grid;
            if (grid == null)
            {
                return 0f;
            }

            var unit = input.unit;

            return grid.origin.y - (unit.position.y - unit.baseToPositionOffset);
        }
    }
}
