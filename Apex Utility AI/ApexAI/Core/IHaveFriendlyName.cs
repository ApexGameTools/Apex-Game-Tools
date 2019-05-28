/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI
{
    /// <summary>
    /// Interface for elements that want to expose a friendly name and description.
    /// </summary>
    public interface IHaveFriendlyName
    {
        /// <summary>
        /// Gets the friendly name.
        /// </summary>
        /// <value>
        /// The friendly name.
        /// </value>
        string friendlyName { get; }

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        string description { get; }
    }
}
