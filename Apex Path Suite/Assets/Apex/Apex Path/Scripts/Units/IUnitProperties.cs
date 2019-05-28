namespace Apex.Units
{
    using Apex.Common;
    using Apex.Steering;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// Interface for basic unit stats
    /// </summary>
    public interface IUnitProperties : IPositioned, IHaveAttributes
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
        /// Gets the ground offset, i..e the distance from the bottom of the unit's collider to the ground.
        /// </summary>
        /// <value>
        /// The ground offset.
        /// </value>
        float groundOffset { get; }

        /// <summary>
        /// Gets the offset between the unit's lower most point where it will touch the ground (touchdownPosition) and its position, typically the bottom of its collider to its position (y delta).
        /// </summary>
        /// <value>
        /// The position to ground offset.
        /// </value>
        float baseToPositionOffset { get; }

        /// <summary>
        /// Gets the height of the unit, i.e. from where it touches the ground to the top of its head (if it has one).
        /// </summary>
        /// <value>
        /// The height of the unit.
        /// </value>
        float height { get; }

        /// <summary>
        /// Gets the position where the unit touches the ground (if it is grounded). This is its position offset by baseToPositionOffset
        /// </summary>
        /// <value>
        /// The touchdown position.
        /// </value>
        Vector3 basePosition { get; }

        /// <summary>
        /// Gets the height navigation capability of the unit, i.e. how steep it can climb etc.
        /// </summary>
        /// <value>
        /// The height navigation capability.
        /// </value>
        HeightNavigationCapabilities heightNavigationCapability { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is selectable.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is selectable; otherwise, <c>false</c>.
        /// </value>
        bool isSelectable { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this unit is selected. Only has an impact if <see cref="isSelectable"/> is true.
        /// </summary>
        /// <value>
        ///   <c>true</c> if selected; otherwise, <c>false</c>.
        /// </value>
        bool isSelected { get; set; }

        /// <summary>
        /// Gets or sets the determination factor used to evaluate whether this unit separates or avoids other units. The higher the determination, the less avoidance/separation.
        /// </summary>
        /// <value>
        /// The determination.
        /// </value>
        int determination { get; set; }

        /// <summary>
        /// Recalculates the base position and unit height. Call this if the unit's size changes at runtime
        /// </summary>
        void RecalculateBasePosition();

        /// <summary>
        /// Marks the unit as pending for selection. This is used to indicate a selection is progress, before the actual selection occurs.
        /// </summary>
        /// <param name="pending">if set to <c>true</c> the unit is pending for selection otherwise it is not.</param>
        void MarkSelectPending(bool pending);
    }
}
