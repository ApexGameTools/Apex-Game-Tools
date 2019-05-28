/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI
{
    using Apex.Serialization;

    /// <summary>
    /// Represents the default Qualifier of a <see cref="Selector"/>. If no other Qualifiers are selected, this is selected.
    /// </summary>
    /// <seealso cref="Apex.AI.QualifierBase" />
    /// <seealso cref="Apex.AI.IDefaultQualifier" />
    [FriendlyName("Default Action"), Hidden]
    public sealed class DefaultQualifier : QualifierBase, IDefaultQualifier
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultQualifier"/> class.
        /// </summary>
        public DefaultQualifier()
        {
            this.score = 1f;
        }

        /// <summary>
        /// Gets or sets the default score.
        /// </summary>
        /// <value>
        /// The score.
        /// </value>
        [ApexSerialization(defaultValue = 1f)]
        public float score
        {
            get;
            set;
        }

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