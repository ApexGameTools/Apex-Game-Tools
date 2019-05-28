/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    using Apex.Steering;

    /// <summary>
    /// Represents the strategy used to create cells in the grid
    /// </summary>
    public sealed class CellConstructionStrategy
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CellConstructionStrategy"/> class.
        /// </summary>
        /// <param name="heightStrategy">The height strategy.</param>
        /// <param name="useClearance">Whether or not to use clearance.</param>
        /// <param name="heightDiffThreshold">The height difference threshold if using clearance.</param>
        public CellConstructionStrategy(HeightStrategy heightStrategy, bool useClearance, float heightDiffThreshold)
        {
            this.calculateHeights = (heightStrategy.heightMode != HeightSamplingMode.NoHeightSampling);

            if (!this.calculateHeights)
            {
                if (useClearance)
                {
                    this.cellFactory = FlatClearanceCell.factory;
                    this.clearanceProvider = new FlatCellClearanceProvider();
                }
                else
                {
                    this.cellFactory = FlatCell.factory;
                }
            }
            else if (heightStrategy.useGlobalHeightNavigationSettings)
            {
                if (useClearance)
                {
                    this.cellFactory = StandardClearanceCell.factory;
                    this.clearanceProvider = new StandardCellClearanceProvider(heightDiffThreshold);
                }
                else
                {
                    this.cellFactory = StandardCell.factory;
                }

                this.heightSettingsProvider = new StandardCellHeightSettingsProvider();
            }
            else
            {
                if (useClearance)
                {
                    this.cellFactory = RichClearanceCell.factory;
                    
                    //We use the same clearance provider the standard cell as clearance is not on a per unit basis.
                    this.clearanceProvider = new StandardCellClearanceProvider(heightDiffThreshold);
                }
                else
                {
                    this.cellFactory = RichCell.factory;
                }

                this.heightSettingsProvider = new RichCellHeightSettingsProvider();
            }
        }

        /// <summary>
        /// Gets a value indicating whether height are calculated for the cells, i.e. is this a flat terrain or not.
        /// </summary>
        public bool calculateHeights
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the actual cell factory.
        /// </summary>
        public ICellFactory cellFactory
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the height settings provider.
        /// </summary>
        public IHeightSettingsProvider heightSettingsProvider
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the clearance provider.
        /// </summary>
        public IClearanceProvider clearanceProvider
        {
            get;
            private set;
        }
    }
}
