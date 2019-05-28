/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Steering
{
    using Apex.Units;
    using UnityEngine;

    /// <summary>
    /// This formation returns positions in an as-square-as-possible grid shape.
    /// </summary>
    public class FormationGrid : IFormation
    {
        private float _formationSpacing = 1.5f;

        /// <summary>
        /// Initializes a new instance of the <see cref="FormationGrid"/> class.
        /// </summary>
        /// <param name="formationSpacing">The formation spacing.</param>
        public FormationGrid(float formationSpacing)
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
            int columns = Mathf.CeilToInt(Mathf.Sqrt(count));
            int rows = Mathf.FloorToInt(count / (float)columns);

            float column = (formationIndex % columns) - (columns * 0.5f);
            float row = Mathf.FloorToInt((formationIndex / columns) - (rows * 0.5f));

            return new Vector3((column * _formationSpacing), 0f, (-row * _formationSpacing));
        }
    }
}