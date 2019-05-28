namespace Apex.Steering.Messages
{
    using Apex.DataStructures;
    using Apex.Units;
    using Apex.Utilities;

    /// <summary>
    /// A message to represent a change in unit selection.
    /// </summary>
    public class UnitsSelectedMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnitsSelectedMessage"/> class.
        /// </summary>
        /// <param name="units">The units selected.</param>
        public UnitsSelectedMessage(IGrouping<IUnitFacade> units)
        {
            Ensure.ArgumentNotNull(units, "units");
            this.selectedUnits = units;
        }

        /// <summary>
        /// Gets the selected units.
        /// </summary>
        /// <value>
        /// The selected units.
        /// </value>
        public IGrouping<IUnitFacade> selectedUnits
        {
            get;
            private set;
        }
    }
}
