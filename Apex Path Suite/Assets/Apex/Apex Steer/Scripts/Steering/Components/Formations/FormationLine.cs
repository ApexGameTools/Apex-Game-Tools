/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Steering
{
    using Apex.Units;
    using UnityEngine;

    /// <summary>
    /// This formation returns positions in a straight line in front of and behind of the model unit.
    /// </summary>
    public class FormationLine : IFormation
    {
        private float _formationSpacing = 1.5f;

        /// <summary>
        /// Initializes a new instance of the <see cref="FormationLine"/> class.
        /// </summary>
        /// <param name="formationSpacing">The formation spacing.</param>
        public FormationLine(float formationSpacing)
        {
            _formationSpacing = formationSpacing;
        }

        /// <summary>
        /// Gets the formation position of this particular formation type. Meant to be used in a loop to generate formation positions for an entire group.
        /// </summary>
        /// <param name="count">The group's member count.</param>
        /// <param name="formationIndex">The unit's formation index - i.e. index in the group. Must never be below 0 if meant to be valid.</param>
        /// <param name="unit">The unit to calculate this particular formation position for.</param>
        /// <returns>
        /// A single formation position based on the count, index and the particular unit.
        /// </returns>
        public Vector3 GetFormationPosition(int count, int formationIndex, IUnitFacade unit)
        {
            float half = (count * 0.5f);
            return new Vector3(0f, 0f, (formationIndex - half) * _formationSpacing);
        }
    }
}