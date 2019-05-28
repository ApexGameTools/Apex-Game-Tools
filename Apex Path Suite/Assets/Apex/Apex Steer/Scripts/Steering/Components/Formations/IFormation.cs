/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Steering
{
    using Apex.Units;
    using UnityEngine;

    /// <summary>
    /// Generic interface for all formation types. If implementing custom formation types, use this interface.
    /// </summary>
    public interface IFormation
    {
        /// <summary>
        /// Gets the formation position of this particular formation type. Meant to be used in a loop to generate formation positions for an entire group.
        /// </summary>
        /// <param name="count">The group's member count.</param>
        /// <param name="formationIndex">The unit's formation index - i.e. index in the group. Must never be below 0 if meant to be valid.</param>
        /// <param name="unit">The unit to calculate this particular formation position for.</param>
        /// <returns>A single formation position based on the count, index and the particular unit.</returns>
        Vector3 GetFormationPosition(int count, int formationIndex, IUnitFacade unit);
    }
}