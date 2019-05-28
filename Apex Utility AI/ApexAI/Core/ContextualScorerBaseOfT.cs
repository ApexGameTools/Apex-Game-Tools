/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI
{
    using Apex.Serialization;

    /// <summary>
    /// Base class for contextual scorers used by <see cref="ICompositeScorer"/>s
    /// </summary>
    /// <typeparam name="TContext">The type of the context.</typeparam>
    /// <seealso cref="Apex.AI.IContextualScorer" />
    public abstract class ContextualScorerBase<TContext> : IContextualScorer where TContext : class, IAIContext
    {
        /// <summary>
        /// The base or default score
        /// </summary>
        [ApexSerialization(defaultValue = 0f)]
        public float score;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is disabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is disabled; otherwise, <c>false</c>.
        /// </value>
        [ApexSerialization(hideInEditor = true, defaultValue = false)]
        public bool isDisabled
        {
            get;
            set;
        }

        /// <summary>
        /// Calculates a score given the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>
        /// The score.
        /// </returns>
        float IContextualScorer.Score(IAIContext context)
        {
            return Score((TContext)context);
        }

        /// <summary>
        /// Calculates a score given the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>
        /// The score.
        /// </returns>
        public abstract float Score(TContext context);
    }
}
