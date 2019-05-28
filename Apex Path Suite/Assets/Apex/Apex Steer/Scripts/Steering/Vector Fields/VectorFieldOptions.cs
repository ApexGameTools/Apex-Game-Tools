/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Steering.VectorFields
{
    using System;
    using Apex.Utilities;

    /// <summary>
    /// The by-default available vector field types.
    /// If new vector field types are added, they need an enum value in this enum as well as being managed in the VectorFieldManagerComponent.
    /// </summary>
    public enum VectorFieldType
    {
        /// <summary>
        /// A full grid vector field
        /// </summary>
        FullGridField,

        /// <summary>
        /// A progressive field, that only exists around the units.
        /// </summary>
        ProgressiveField,

        /// <summary>
        /// A cross grid field that provides support for stitched together grids.
        /// </summary>
        CrossGridField,

        /// <summary>
        /// A funnel field that makes a narrow funnel from start to goal.
        /// </summary>
        FunnelField,
    }

    /// <summary>
    /// A class solely responsible for exposing the vector field options.
    /// Has its own custom editor.
    /// </summary>
    [Serializable]
    public class VectorFieldOptions
    {
        /// <summary>
        /// The desired vector field type
        /// </summary>
        [Label("Vector Field Type", "The desired vector field type.")]
        public VectorFieldType vectorFieldType = VectorFieldType.FunnelField;

        /// <summary>
        /// If true, treats missing cells the same as blocked cells. This means that the edge cells on the grid will never point in a direction leaving the grid.
        /// </summary>
        [Label("Built-In Containment?", "If units are not supposed to leave the grid that they are on (except through portals), set to true, otherwise false.")]
        public bool builtInContainment = true;

        /// <summary>
        /// How many percentages (0.5 = 50%) the group bounds is expected to grow. Set to higher value if you expect a large separation between units.
        /// </summary>
        [MinCheck(0f, label = "Expected Group Growth Factor", tooltip = "How many percentages (0.5 = 50%) the group bounds is expected to grow. Set to higher value if you expect a large separation between units.")]
        public float expectedGroupGrowthFactor = 0.5f;

        /// <summary>
        /// How often the load balancer executes the vector field. Set to lower value for better behaviour, while higher values give better performance.
        /// </summary>
        [MinCheck(0.1f, label = "Update Interval", tooltip = "How often the load balancer executes the vector field. Set to lower value for better behaviour, while higher values give better performance.")]
        public float updateInterval = 0.5f;

        /// <summary>
        /// How much to increase the group bounds by per increase.
        /// </summary>
        [MinCheck(0.1f, label = "Bounds Padding Increase", tooltip = "How much to increase the group bounds by per increase.")]
        public float paddingIncrease = 2f;

        /// <summary>
        /// The maximum value for group bounds padding increase, e.g. how much the group bounds can be increased.
        /// </summary>
        [MinCheck(0f, label = "Max Bounds Padding", tooltip = "The maximum value for group bounds padding increase, e.g. how much the group bounds can be increased.")]
        public float maxExtraPadding = 30f;

        /// <summary>
        /// What value the group bounds padding (the extra space added to group bounds) starts at.
        /// </summary>
        [MinCheck(0.01f, label = "Start Bounds Padding", tooltip = "What value the group bounds padding (the extra space added to group bounds) starts at.")]
        public float boundsPadding = 7f;

        /// <summary>
        /// The distance that the group's center of gravity needs to move, before the vector field is re-calculated. Set to lower value for better performance, and higher value for better behaviour.
        /// </summary>
        [MinCheck(0.1f, label = "Bounds Recalculate Threshold", tooltip = "The distance that the group's center of gravity needs to move, before the vector field is re-calculated. Set to lower value for better performance, and higher value for better behaviour.")]
        public float boundsRecalculateThreshold = 2f;

        /// <summary>
        /// A factor that is used to multiply the magnitude of vector field vectors neighbouring blocked cells, in order for them to have a higher contribution in the smoothing pass.
        /// </summary>
        [MinCheck(1f, label = "Obstacle Strength Factor", tooltip = "A factor that is used to multiply the magnitude of vector field vectors neighbouring blocked cells, in order for them to have a higher contribution in the smoothing pass.")]
        public float obstacleStrengthFactor = 10f;

        /// <summary>
        /// The distance from the path in meters that the funnel vector field uses for its funnel width.
        /// </summary>
        [MinCheck(2f, label = "Funnel Width", tooltip = "The distance from the path in meters that the funnel vector field uses for its funnel width.")]
        public float funnelWidth = 10f;
    }
}