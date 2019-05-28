/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI
{
    using System.Collections.Generic;
    using Apex.Serialization;

    /// <summary>
    /// A composite qualifier whose score is either the sum off all its child scorers or 0 if one or more scorers score below the threshold.
    /// </summary>
    /// <seealso cref="Apex.AI.CompositeQualifier" />
    [AICategory("Composite Qualifiers"), FriendlyName("All or Nothing", "Only scores if all child scorers score above the threshold.")]
    public class CompositeAllOrNothingQualifier : CompositeQualifier
    {
        /// <summary>
        /// The threshold, which if any child scorer scores below this, the score of this qualifier will be 0.
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
                    return 0f;
                }

                sum += score;
            }

            return sum;
        }
    }
}
