/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI
{
    /// <summary>
    /// Interface for contextual scorers. 
    /// </summary>
    /// <seealso cref="Apex.AI.ICanBeDisabled" />
    /// <seealso cref="ICompositeScorer"/>
    public interface IContextualScorer : ICanBeDisabled
    {
        /// <summary>
        /// Calculates a score given the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>The score.</returns>
        float Score(IAIContext context);
    }
}
