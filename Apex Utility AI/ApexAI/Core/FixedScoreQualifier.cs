namespace Apex.AI
{
    using Apex.Serialization;

    /// <summary>
    /// A qualifier that always returns the same score.
    /// </summary>
    /// <seealso cref="Apex.AI.QualifierBase" />
    [FriendlyName("Fixed Score", "Always scores a fixed score.")]
    public class FixedScoreQualifier : QualifierBase
    {
        /// <summary>
        /// The score
        /// </summary>
        [ApexSerialization(defaultValue = 2f)]
        public float score = 2f;

        /// <summary>
        /// Calculates the score for this qualifier given the provided context. This will always be <see cref="score"/>
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>
        /// The score.
        /// </returns>
        public override float Score(IAIContext context)
        {
            return this.score;
        }
    }
}