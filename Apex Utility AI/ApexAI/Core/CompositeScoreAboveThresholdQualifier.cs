/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI
{
    using System.Collections.Generic;
    using Apex.Serialization;

    /// <summary>
    /// A composite qualifier whose score is either the sum off all its child scorers or 0 if the sum equals or falls below the threshold.
    /// </summary>
    /// <seealso cref="Apex.AI.CompositeQualifier" />
    [AICategory("Composite Qualifiers"), FriendlyName("Minimum or Nothing", "Scores by summing child scores. Only returns a score if the sum exceeds the threshold, otherwise 0 is returned.")]
    public class CompositeScoreAboveThresholdQualifier : CompositeQualifier
    {
        /// <summary>
        /// The threshold, which the score must be above to score out.
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

                sum += scorer.Score(context);
            }

            return sum > threshold ? sum : 0f;
        }
    }
}