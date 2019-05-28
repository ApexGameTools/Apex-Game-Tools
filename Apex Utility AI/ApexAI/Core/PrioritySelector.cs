/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI
{
    using System.Collections.Generic;

    /// <summary>
    /// A Selector that choses the first <see cref="IQualifier"/> to score above the <see cref="Selector.defaultQualifier"/>.
    /// </summary>
    /// <seealso cref="Apex.AI.Selector" />
    [FriendlyName("First Score Wins", "The first qualifier to score above the score of the Default Qualifier, is selected.")]
    public class PrioritySelector : Selector
    {
        /// <summary>
        /// Selects the action for execution, given the specified context.
        /// This selector choses the first <see cref="IQualifier"/> to score above the <see cref="Selector.defaultQualifier"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="qualifiers">The qualifiers from which to find the action.</param>
        /// <param name="defaultQualifier">The default qualifier.</param>
        /// <returns>
        /// The qualifier whose action should be chosen.
        /// </returns>
        public override IQualifier Select(IAIContext context, IList<IQualifier> qualifiers, IDefaultQualifier defaultQualifier)
        {
            var count = qualifiers.Count;
            var threshold = defaultQualifier.score;

            for (int i = 0; i < count; i++)
            {
                var qualifier = qualifiers[i];
                if (qualifier.isDisabled)
                {
                    continue;
                }

                var score = qualifier.Score(context);
                if (score > threshold)
                {
                    return qualifier;
                }
            }

            return defaultQualifier;
        }
    }
}