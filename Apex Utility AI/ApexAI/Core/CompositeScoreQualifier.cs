/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI
{
    using System.Collections.Generic;

    /// <summary>
    /// A composite qualifier whose score is the sum of child scorers' scores.
    /// </summary>
    /// <seealso cref="Apex.AI.CompositeQualifier" />
    [AICategory("Composite Qualifiers"), FriendlyName("Sum of Children", "Scores by summing the score of all child scorers.")]
    public class CompositeScoreQualifier : CompositeQualifier
    {
        /// <summary>
        /// Calculates a score using the supplied scorers given the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="scorers">The scorers.</param>
        /// <returns>
        /// The score.
        /// </returns>
        public sealed override float Score(IAIContext context, IList<IContextualScorer> scorers)
        {
            float sum = 0f;

            int scorersCount = scorers.Count;
            for (int i = 0; i < scorersCount; i++)
            {
                var scorer = scorers[i];
                if (scorer.isDisabled)
                {
                    continue;
                }

                sum += scorer.Score(context);
            }

            return sum;
        }
    }
}
