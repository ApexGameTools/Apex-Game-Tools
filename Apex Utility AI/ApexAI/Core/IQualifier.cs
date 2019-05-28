/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI
{
    /// <summary>
    /// Interface for Qualifiers
    /// </summary>
    /// <seealso cref="Apex.AI.ICanBeDisabled" />
    public interface IQualifier : ICanBeDisabled
    {
        /// <summary>
        /// Gets or sets the associated action.
        /// </summary>
        /// <value>
        /// The action.
        /// </value>
        IAction action { get; set; }

        /// <summary>
        /// Calculates the score for this qualifier given the provided context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>The score.</returns>
        float Score(IAIContext context);
    }
}