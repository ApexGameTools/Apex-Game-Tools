/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Steering
{
    using Apex.Units;
    using UnityEngine;

    /// <summary>
    /// This formation returns positions in a circular shape - therefore the name 'Ellipsoid'.
    /// </summary>
    public class FormationEllipsoid : IFormation
    {
        private float _formationRadius = 3f;

        /// <summary>
        /// Initializes a new instance of the <see cref="FormationEllipsoid"/> class.
        /// </summary>
        /// <param name="formationRadius">The formation radius.</param>
        public FormationEllipsoid(float formationRadius)
        {
            _formationRadius = formationRadius;
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
            float radius = count / (2f * Mathf.PI) * _formationRadius;
            float step = (2f * Mathf.PI) / count;

            float t = step * formationIndex;
            float x = Mathf.Cos(t);
            float z = Mathf.Sin(t);

            return new Vector3(x, 0f, z) * radius;
        }
    }
}