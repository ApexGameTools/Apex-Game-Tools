namespace Apex.Units
{
    using Apex.Common;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// Interface for basic unit stats
    /// </summary>
    public interface IUnit : IPositioned, IHaveAttributes
    {
        /// <summary>
        /// Gets the radius of the unit
        /// </summary>
        /// <value>
        /// The radius of the unit
        /// </value>
        float radius { get; }

        /// <summary>
        /// Gets the unit's field of view in degrees.
        /// </summary>
        /// <value>
        /// The field of view.
        /// </value>
        float fieldOfView { get; }

        /// <summary>
        /// Gets the forward vector of the unit, i.e. the direction its nose is pointing (provided it has a nose).
        /// </summary>
        /// <value>
        /// The forward vector.
        /// </value>
        Vector3 forward { get; }
    }
}
