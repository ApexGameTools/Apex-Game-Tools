/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI
{
    /// <summary>
    /// Interface for entities that can be disabled
    /// </summary>
    public interface ICanBeDisabled
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is disabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is disabled; otherwise, <c>false</c>.
        /// </value>
        bool isDisabled { get; set; }
    }
}
