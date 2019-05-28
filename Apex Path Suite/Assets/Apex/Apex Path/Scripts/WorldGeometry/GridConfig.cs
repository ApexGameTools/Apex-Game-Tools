namespace Apex.WorldGeometry
{
    using Apex.DataStructures;
    using UnityEngine;

    /// <summary>
    /// Configuration settings for grids created at runtime using <see cref="GridComponent.Create(UnityEngine.GameObject, GridConfig)"/>
    /// </summary>
    public sealed class GridConfig
    {
        private float _connectorPortalWidth = 0f;

        /// <summary>
        /// Initializes a new instance of the <see cref="GridConfig"/> class.
        /// </summary>
        public GridConfig()
        {
            this.sizeX = 10;
            this.sizeZ = 10;
            this.cellSize = 2f;
            this.generateHeightmap = true;
            this.heightLookupType = HeightLookupType.Dictionary;
            this.heightLookupMaxDepth = 5;
            this.lowerBoundary = 1f;
            this.upperBoundary = 10f;
            this.obstacleSensitivityRange = 0.5f;
            this.obstacleAndGroundDetection = ColliderDetectionMode.Mixed;
            this.automaticConnections = false;
            this.subSectionsCellOverlap = 2;
            this.subSectionsX = 2;
            this.subSectionsZ = 2;
        }

        /// <summary>
        /// Gets or sets the friendly name of the grid. Used in messages and such.
        /// </summary>
        public string friendlyName { get; set; }

        /// <summary>
        /// The origin, i.e. center of the grid
        /// </summary>
        public Vector3 origin { get; set; }

        /// <summary>
        /// size along the x-axis.
        /// </summary>
        public int sizeX { get; set; }

        /// <summary>
        /// size along the z-axis.
        /// </summary>
        public int sizeZ { get; set; }

        /// <summary>
        /// The cell size.
        /// </summary>
        public float cellSize { get; set; }

        /// <summary>
        /// Gets or sets the width of the connector portals. This is used when auto-connecting grids.
        /// The default is the <see cref="cellSize"/>, but if larger units need to be able to traverse a connector the width must be set accordingly.
        /// </summary>
        public float connectorPortalWidth
        {
            get
            {
                if (_connectorPortalWidth <= 0f)
                {
                    return cellSize;
                }

                return _connectorPortalWidth;
            }

            set
            {
                _connectorPortalWidth = value;
            }
        }

        /// <summary>
        /// The obstacle sensitivity range, meaning any obstacle within this range of the cell center will cause the cell to be blocked.
        /// </summary>
        public float obstacleSensitivityRange { get; set; }

        /// <summary>
        /// The obstacle and ground detection mode used when determining the terrain and obstacles of the grid.
        /// </summary>
        public ColliderDetectionMode obstacleAndGroundDetection { get; set; }

        /// <summary>
        /// Gets or sets the obstacle and ground detector. This can be set to a custom implementation in conjunction with <see cref="obstacleAndGroundDetection"/> set to Custom.
        /// </summary>
        public IBlockDetector obstacleAndGroundDetector { get; set; }

        /// <summary>
        /// Controls whether the grid will automatically attempt to connect itself with adjacent grids once it has initialized.\nWill only connect to other initialized and enabled grids.
        /// </summary>
        public bool automaticConnections { get; set; }

        /// <summary>
        /// Whether or not to generate a height map to enable units to follow a terrain of differing heights.
        /// </summary>
        public bool generateHeightmap { get; set; }

        /// <summary>
        /// Gets the type of the height lookup. Dictionaries are fast but memory hungry. Quad Tree stats depend on height density.
        /// </summary>
        /// <value>
        /// The type of the height lookup.
        /// </value>
        public HeightLookupType heightLookupType { get; set; }

        /// <summary>
        /// Gets the height lookup maximum depth. Only applicable to Quad Trees.
        /// </summary>
        /// <value>
        /// The height lookup maximum depth.
        /// </value>
        public int heightLookupMaxDepth { get; set; }

        /// <summary>
        /// The upper boundary (y - value) of the matrix.
        /// </summary>
        public float upperBoundary { get; set; }

        /// <summary>
        /// The lower boundary (y - value) of the matrix.
        /// </summary>
        public float lowerBoundary { get; set; }

        /// <summary>
        /// The sub sections along the x-axis.
        /// </summary>
        public int subSectionsX { get; set; }

        /// <summary>
        /// The sub sections along the z-axis.
        /// </summary>
        public int subSectionsZ { get; set; }

        /// <summary>
        /// The sub sections cell overlap
        /// </summary>
        public int subSectionsCellOverlap { get; set; }
    }
}
