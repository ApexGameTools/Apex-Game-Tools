namespace Apex.AI
{
    using System.Collections.Generic;
    using Apex.Serialization;

    /// <summary>
    /// Composite qualifier whose score is the sum of scorer scores until one scorer scores below the threshold.
    /// </summary>
    /// <seealso cref="Apex.AI.CompositeQualifier" />
    [AICategory("Composite Qualifiers"), FriendlyName("Sum while above threshold", "Scores by summing child scores, until a child scores below the threshold")]
    public class CompositePriorityQualifier : CompositeQualifier
    {
        /// <summary>
        /// The threshold, which will stop summation of scores if/once the first scorer scores below this.
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
                    return sum;
                }

                sum += score;
            }

            return sum;
        }
    }
}