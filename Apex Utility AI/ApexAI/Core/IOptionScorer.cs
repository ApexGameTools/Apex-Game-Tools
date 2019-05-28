/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI
{
    /// <summary>
    /// Interface for options scorers.
    /// </summary>
    /// <typeparam name="T">The type of the option.</typeparam>
    /// <seealso cref="Apex.AI.ICanBeDisabled" />
    public interface IOptionScorer<T> : ICanBeDisabled
    {
        /// <summary>
        /// Calculates the score for the option given the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="option">The option to score.</param>
        /// <returns>The score.</returns>
        float Score(IAIContext context, T option);
    }
}
