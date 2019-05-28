/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering
{
    using Apex.Services;
    using Apex.Steering;
    using Apex.Utilities;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// Component for setting overall navigation settings
    /// </summary>
    [ExecuteInEditMode]
    [AddComponentMenu("Apex/Game World/Navigation Settings", 1037)]
    [ApexComponent("Game World")]
    public partial class NavigationSettingsComponent : MonoBehaviour
    {
        /// <summary>
        /// The way by which height sampling and height navigation is performed
        /// </summary>
        [Label("Mode", "The way by which height sampling and height navigation is performed.")]
        public HeightSamplingMode heightSampling = HeightSamplingMode.HeightMap;

        /// <summary>
        /// The detail level of the height map, Normal is recommended. High is more accurate but somewhat slower to generate.
        /// </summary>
        [Label("Height Map Detail", "The detail level of the height map, Normal is recommended. High is more accurate but somewhat slower to generate.")]
        public HeightMapDetailLevel heightMapDetail = HeightMapDetailLevel.Normal;

        /// <summary>
        /// The distance between height samples.
        /// </summary>
        [MinCheck(0.05f, label = "Granularity", tooltip = "The distance between height samples.")]
        public float heightSamplingGranularity = 0.1f;

        /// <summary>
        /// The max angle at which a piece of geometry is considered a ledge. A climb or drop is defined as movement from one ledge to another.
        /// </summary>
        [RangeX(0f, 89f, label = "Ledge Threshold", tooltip = "The max angle at which a piece of geometry is considered a ledge. A climb or drop is defined as movement from one ledge to another.")]
        public float ledgeThreshold = 10f;

        /// <summary>
        /// Controls whether units define their own height navigation capabilities or use a global setting.
        /// </summary>
        [Tooltip("Controls whether units define their own height navigation capabilities or use a global setting.")]
        public bool useGlobalHeightNavigationSettings = true;

        /// <summary>
        /// The global unit height navigation capability
        /// </summary>
        public HeightNavigationCapabilities unitsHeightNavigationCapability = new HeightNavigationCapabilities
        {
            maxClimbHeight = 0.5f,
            maxDropHeight = 1f,
            maxSlopeAngle = 30f
        };

        /// <summary>
        /// Controls whether a clearance map is generated to allow for differently sized units.
        /// </summary>
        [Tooltip("Controls whether a clearance map is generated to allow for differently sized units.")]
        public bool useClearance = false;

        /// <summary>
        /// Represents the maximum height difference between adjacent cells before they will see each other as blocked with regards to clearance.
        /// </summary>
        [Tooltip("Represents the maximum height difference between adjacent cells before they will see each other as blocked with regards to clearance.")]
        public float heightDiffThreshold = 0.5f;

        private void OnEnable()
        {
            Refresh();
        }

        /// <summary>
        /// For internal use, do not call this.
        /// </summary>
        public void Refresh() 
        {
            GameServices.heightStrategy = new HeightStrategy(
                this.heightSampling,
                this.heightSamplingGranularity,
                this.ledgeThreshold,
                this.useGlobalHeightNavigationSettings,
                this.unitsHeightNavigationCapability,
                this.heightMapDetail);

            GameServices.cellConstruction = new CellConstructionStrategy(GameServices.heightStrategy, this.useClearance, this.heightDiffThreshold);

            RefreshPartial();
        }

        partial void RefreshPartial();
    }
}
