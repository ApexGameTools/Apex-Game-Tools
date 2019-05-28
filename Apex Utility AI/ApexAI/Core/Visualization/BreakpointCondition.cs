/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Visualization
{
    /// <summary>
    /// Defines a break point condition based on a score.
    /// </summary>
    public sealed class BreakpointCondition
    {
        /// <summary>
        /// The score threshold. The score is compared to this.
        /// </summary>
        public float scoreThreshold;

        /// <summary>
        /// The compare operator
        /// </summary>
        public CompareOperator compareOperator = CompareOperator.Equals;

        /// <summary>
        /// Evaluates the specified score.
        /// </summary>
        /// <param name="score">The score.</param>
        /// <returns><c>true</c> if the score matches the compare criteria; otherwise <c>false</c></returns>
        public bool Evaluate(float? score)
        {
            if (!score.HasValue)
            {
                return false;
            }

            switch (this.compareOperator)
            {
                case CompareOperator.GreaterThan:
                {
                    return score > scoreThreshold;
                }

                case CompareOperator.LessThan:
                {
                    return score < scoreThreshold;
                }

                case CompareOperator.GreaterThanOrEquals:
                {
                    return score >= scoreThreshold;
                }

                case CompareOperator.LessThanOrEquals:
                {
                    return score <= scoreThreshold;
                }

                case CompareOperator.Equals:
                {
                    return score == scoreThreshold;
                }

                default:
                {
                    return score != scoreThreshold;
                }
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            switch (this.compareOperator)
            {
                case CompareOperator.GreaterThan:
                {
                    return string.Concat("Score > ", this.scoreThreshold);
                }

                case CompareOperator.LessThan:
                {
                    return string.Concat("Score < ", this.scoreThreshold);
                }

                case CompareOperator.GreaterThanOrEquals:
                {
                    return string.Concat("Score >= ", this.scoreThreshold);
                }

                case CompareOperator.LessThanOrEquals:
                {
                    return string.Concat("Score <= ", this.scoreThreshold);
                }

                case CompareOperator.Equals:
                {
                    return string.Concat("Score == ", this.scoreThreshold);
                }

                default:
                {
                    return string.Concat("Score != ", this.scoreThreshold);
                }
            }
        }
    }
}
