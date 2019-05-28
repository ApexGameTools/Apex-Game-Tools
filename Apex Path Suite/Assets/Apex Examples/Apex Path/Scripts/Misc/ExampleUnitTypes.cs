namespace Apex.Examples.Misc
{
    using System;
    using Apex.Common;

    /// <summary>
    /// An example of creating the custom attributes to be associated with <see cref="Apex.Common.AttributedComponent"/>s and other attribute using components.
    /// </summary>
    [Flags, EntityAttributesEnum]
    public enum ExampleUnitTypes
    {
        /// <summary>
        /// Entities on orange team
        /// </summary>
        OnOrangeTeam = 1,

        /// <summary>
        /// Entities on green team
        /// </summary>
        OnGreenTeam = 2,

        /// <summary>
        /// Entities on yellow team
        /// </summary>
        OnYellowTeam = 4,

        /// <summary>
        /// Special doors are available
        /// </summary>
        SpecialDoorsAvailable = 512
    }
}
