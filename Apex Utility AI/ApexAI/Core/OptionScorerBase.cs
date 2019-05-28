/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI
{
    using Apex.Serialization;

    /// <summary>
    /// Base class for option scorers used by <see cref="ActionWithOptions{TOption}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Apex.AI.IOptionScorer{T}" />
    public abstract class OptionScorerBase<T> : IOptionScorer<T>
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
        public abstract float Score(IAIContext context, T option);
    }
}
