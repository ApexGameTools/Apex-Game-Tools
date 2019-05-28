/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI
{
    using Apex.Serialization;

    /// <summary>
    /// Base class for option scorers used by <see cref="ActionWithOptions{TOption}"/>
    /// </summary>
    /// <typeparam name="T">The type of the option</typeparam>
    /// <typeparam name="TContext">The type of the context.</typeparam>
    /// <seealso cref="Apex.AI.IOptionScorer{T}" />
    public abstract class OptionScorerBase<T, TContext> : IOptionScorer<T> where TContext : class, IAIContext
    {
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
        /// Calculates the score for the option given the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="option">The option to score.</param>
        /// <returns>
        /// The score.
        /// </returns>
        float IOptionScorer<T>.Score(IAIContext context, T option)
        {
            return Score((TContext)context, option);
        }

        /// <summary>
        /// Calculates the score for the option given the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="option">The option to score.</param>
        /// <returns>
        /// The score.
        /// </returns>
        public abstract float Score(TContext context, T option);
    }
}
