/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Common
{
    /// <summary>
    /// Interface for entities that have attributes
    /// </summary>
    public interface IHaveAttributes
    {
        /// <summary>
        /// Gets the attributes.
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        AttributeMask attributes { get; }
    }
}
