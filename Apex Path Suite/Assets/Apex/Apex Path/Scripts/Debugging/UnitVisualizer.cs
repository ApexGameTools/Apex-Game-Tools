/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Debugging
{
    using Apex.PathFinding;
    using Apex.Steering;
    using Apex.Steering.Components;
    using Apex.Units;
    using UnityEngine;

    /// <summary>
    /// Visualization component that draws gizmos to represent a moving unit's radius and field of view.
    /// </summary>
    [AddComponentMenu("Apex/Unit/Debugging/Unit Visualizer", 1025)]
    [ApexComponent("Debugging")]
    public partial class UnitVisualizer : Visualizer
    {
        /// <summary>
        /// The radius color
        /// </summary>
        public Color radiusColor = new Color(98f / 255f, 93f / 255f, 227f / 255f);

        private IUnitProperties _unit;
        private Transform _transform;

        /// <summary>
        /// Called on start
        /// </summary>
        protected override void Start()
        {
            _unit = this.GetUnitFacade();
            _transform = this.transform;
        }

        /// <summary>
        /// Draws the actual visualization.
        /// </summary>
        protected override void DrawVisualization()
        {
            if (_unit == null)
            {
                _unit = (IUnitProperties)GetComponent<UnitComponent>();
                _transform = this.transform;
                if (_unit == null)
                {
                    return;
                }
            }

            var pos = _transform.position;
            Gizmos.color = this.radiusColor;
            Gizmos.DrawWireSphere(pos, _unit.radius);
        }
    }
}
