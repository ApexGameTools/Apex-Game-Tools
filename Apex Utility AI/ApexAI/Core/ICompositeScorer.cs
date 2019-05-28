/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI
{
    using System.Collections.Generic;

    /// <summary>
    /// Interface for scorers that themselves calculate a score based on the scores of <see cref="IContextualScorer"/>s.
    /// </summary>
    /// <seealso cref="Apex.AI.CompositeQualifier" />
    public interface ICompositeScorer
    {
        /// <summary>
        /// Gets the scorers that this composite uses.
        /// </summary>
        /// <value>
        /// The scorers.
        /// </value>
        IList<IContextualScorer> scorers { get; }

        /// <summary>
        /// Calculates a score using the supplied scorers given the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="scorers">The scorers.</param>
        /// <returns>The score.</returns>
        float Score(IAIContext context, IList<IContextualScorer> scorers);
    }
}
