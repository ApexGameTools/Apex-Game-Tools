/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI
{
    using Apex.AI.Serialization;

    /// <summary>
    /// Interface for default qualifiers.
    /// </summary>
    /// <seealso cref="Apex.AI.IQualifier" />
    public interface IDefaultQualifier : IQualifier
    {
        /// <summary>
        /// Gets or sets the default score.
        /// </summary>
        /// <value>
        /// The score.
        /// </value>
        float score
        {
            get;
            set;
        }
    }
}