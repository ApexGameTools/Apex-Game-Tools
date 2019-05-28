/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI
{
    using System.Collections.Generic;
    using Apex.Serialization;

    /// <summary>
    /// Composite qualifier whose score is the sum of scorer scores that are above a threshold.
    /// </summary>
    /// <seealso cref="Apex.AI.CompositeQualifier" />
    [AICategory("Composite Qualifiers"), FriendlyName("Sum all above threshold", "Scores by summing child scores that score above the threshold")]
    public class CompositeAllAboveThresholdQualifier : CompositeQualifier
    {
        /// <summary>
        /// The threshold, which is required for a score to be included in the final score.
        /// </summary>
        [ApexSerialization(defaultValue = 0f)]
        public float threshold;

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

                var score = scorer.Score(context);
                if (score <= this.threshold)
                {
                    continue;
                }

                sum += score;
            }

            return sum;
        }
    }
}