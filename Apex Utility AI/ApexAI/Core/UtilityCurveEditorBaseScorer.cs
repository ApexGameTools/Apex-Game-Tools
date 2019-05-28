namespace Apex.AI
{
    using Apex.Serialization;
    using UnityEngine;

    /// <summary>
    /// A base class for contextual scorers that score based on Utility Curves.
    /// </summary>
    /// <seealso cref="Apex.AI.ContextualScorerBase" />
    public abstract class UtilityCurveEditorBaseScorer : ContextualScorerBase
    {
        /// <summary>
        /// The Utility Curve
        /// </summary>
        [ApexSerialization]
        public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        /// <summary>
        /// Gets the curve score.
        /// </summary>
        /// <param name="value">The value from which to get a score.</param>
        /// <returns>The curve score.</returns>
        public float GetCurveScore(float value)
        {
            return curve.Evaluate(value);
        }
    }
}