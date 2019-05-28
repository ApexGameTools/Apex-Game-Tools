/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Serialization
{
    /// <summary>
    /// Staged representation of <c>null</c>
    /// </summary>
    /// <seealso cref="Apex.Serialization.StageItem" />
    public sealed class StageNull : StageItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StageNull"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public StageNull(string name)
            : base(name)
        {
        }
    }
}
