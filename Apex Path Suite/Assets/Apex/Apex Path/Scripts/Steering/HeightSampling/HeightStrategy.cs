/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering
{
    using Apex.WorldGeometry;

    /// <summary>
    /// Represent a height strategy
    /// </summary>
    public sealed class HeightStrategy
    {
        private readonly ISampleHeights _sampler;

        /// <summary>
        /// Initializes a new instance of the <see cref="HeightStrategy"/> class.
        /// </summary>
        /// <param name="mode">The mode.</param>
        /// <param name="sampleGranularity">The sample granularity.</param>
        /// <param name="ledgeThreshold">The ledge threshold.</param>
        /// <param name="useGlobalHeightNavigationSettings">if set to <c>true</c> global height navigation settings will be used.</param>
        /// <param name="unitsHeightNavigationCapability">The global unit height navigation capability.</param>
        /// <param name="heightMapDetail">The height map detail.</param>
        public HeightStrategy(HeightSamplingMode mode, float sampleGranularity, float ledgeThreshold, bool useGlobalHeightNavigationSettings, HeightNavigationCapabilities unitsHeightNavigationCapability, HeightMapDetailLevel heightMapDetail)
        {
            switch (mode)
            {
                case HeightSamplingMode.HeightMap:
                {
                    _sampler = new HeightMapSampler();
                    break;
                }

                case HeightSamplingMode.Raycast:
                {
                    _sampler = new RaycastSampler();
                    break;
                }

                default:
                case HeightSamplingMode.NoHeightSampling:
                {
                    _sampler = new NullSampler();
                    break;
                }
            }

            this.heightMode = mode;
            this.sampleGranularity = sampleGranularity;
            this.ledgeThreshold = ledgeThreshold;
            this.useGlobalHeightNavigationSettings = useGlobalHeightNavigationSettings;
            this.unitsHeightNavigationCapability = unitsHeightNavigationCapability;
            this.heightMapDetail = heightMapDetail;
        }

        /// <summary>
        /// Gets the height sampler.
        /// </summary>
        /// <value>
        /// The height sampler.
        /// </value>
        public ISampleHeights heightSampler
        {
            get { return _sampler; }
        }

        /// <summary>
        /// Gets the height mode.
        /// </summary>
        /// <value>
        /// The height mode.
        /// </value>
        public HeightSamplingMode heightMode
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the height map detail.
        /// </summary>
        /// <value>
        /// The height map detail.
        /// </value>
        public HeightMapDetailLevel heightMapDetail
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the sample granularity.
        /// </summary>
        /// <value>
        /// The sample granularity.
        /// </value>
        public float sampleGranularity
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the ledge threshold.
        /// </summary>
        /// <value>
        /// The ledge threshold.
        /// </value>
        public float ledgeThreshold
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether to use global height navigation settings.
        /// </summary>
        /// <value>
        /// <c>true</c> if using global height navigation settings; otherwise, <c>false</c>.
        /// </value>
        public bool useGlobalHeightNavigationSettings
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the global unit height navigation capability.
        /// </summary>
        /// <value>
        /// The global unit height navigation capability.
        /// </value>
        public HeightNavigationCapabilities unitsHeightNavigationCapability
        {
            get;
            private set;
        }
    }
}
