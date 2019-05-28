/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering
{
    /// <summary>
    /// Interface for <see cref="IMoveUnits"/> factories
    /// </summary>
    public interface IMoveUnitsFactory
    {
        /// <summary>
        /// Creates the mover instance.
        /// </summary>
        /// <returns>The mover instance</returns>
        IMoveUnits Create();
    }
}
