/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Steering
{
    using Apex.Units;
    using UnityEngine;

    /// <summary>
    /// This formation returns position in a wing or 'V' shape, i.e. two straight lines protruding respectively left and back, and right and back.
    /// </summary>
    public class FormationWing : IFormation
    {
        private float _formationSpacing = 1.5f;

        /// <summary>
        /// Initializes a new instance of the <see cref="FormationWing"/> class.
        /// </summary>
        /// <param name="formationSpacing">The formation spacing.</param>
        public FormationWing(float formationSpacing)
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
            float forward = Mathf.Abs((count * 0.5f) - formationIndex) * -0.5f * _formationSpacing;
            float right = ((count * 0.5f) - formationIndex) * -_formationSpacing;

            return new Vector3(right, 0f, forward);
        }
    }
}