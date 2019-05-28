/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Units
{
    using Apex.PathFinding;
    using Apex.Steering;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// Interface for the Unit Facade representing a units abilities
    /// </summary>
    public interface IUnitFacade : IUnitProperties, IMovable, IMovingObject, IDefineSpeed, IGameObjectComponent, IGroupable<IUnitFacade>
    {
        /// <summary>
        /// Gets the unit's collider.
        /// </summary>
        /// <value>
        /// The collider.
        /// </value>
        Collider collider { get; }

        /// <summary>
        /// Gets the unit's transform.
        /// </summary>
        /// <value>
        /// The transform.
        /// </value>
        Transform transform { get; }

        /// <summary>
        /// Gets the forward vector of the unit, i.e. the direction its nose is pointing (provided it has a nose).
        /// </summary>
        /// <value>
        /// The forward vector.
        /// </value>
        Vector3 forward { get; }

        /// <summary>
        /// Gets or sets a point to look at. This does nothing by itself but enables e.g. <see cref="OrientationComponent"/>s to respond to this.
        /// </summary>
        Vector3? lookAt { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is movable, i.e. if the <see cref="IMovable"/> interface is available.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is movable; otherwise, <c>false</c>.
        /// </value>
        bool isMovable { get; }

        /// <summary>
        /// Gets a value indicating whether this unit is alive. If the unit is not alive accessing other properties and methods will cause exceptions.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is alive; otherwise, <c>false</c>.
        /// </value>
        bool isAlive { get; set; }

        /// <summary>
        /// Gets the path finder options to use for issuing path requests.
        /// </summary>
        /// <value>
        /// The path finder options.
        /// </value>
        IPathFinderOptions pathFinderOptions { get; }

        /// <summary>
        /// Gets the path navigation options.
        /// </summary>
        /// <value>
        /// The path navigation options.
        /// </value>
        IPathNavigationOptions pathNavigationOptions { get; }

        /// <summary>
        /// Gets or sets the unit's desired formation position.
        /// </summary>
        /// <value>
        /// The formation position.
        /// </value>
        IPositioned formationPos { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has arrived at its formation position or the vector field's final destination, depending on whether there is a valid formation.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has arrived at its position; otherwise, <c>false</c>.
        /// </value>
        bool hasArrivedAtDestination { get; set; }

        /// <summary>
        /// Initializes the Unit Facade.
        /// </summary>
        /// <param name="unitObject">The unit game object.</param>
        void Initialize(GameObject unitObject);
    }
}