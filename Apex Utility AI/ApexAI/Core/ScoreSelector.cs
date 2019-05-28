/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI
{
    using System.Collections.Generic;

    /// <summary>
    /// A Selector that choses the highest scoring <see cref="IQualifier"/>.
    /// </summary>
    /// <seealso cref="Apex.AI.Selector" />
    [FriendlyName("Highest Score Wins", "The qualifier with the highest score is selected")]
    public class ScoreSelector : Selector
    {
        /// <summary>
        /// Selects the action for execution, given the specified context.
        /// This selector choses the highest scoring <see cref="IQualifier"/>.
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

            float max = defaultQualifier.score;
            IQualifier highRoller = null;

            for (int i = 0; i < count; i++)
            {
                var qualifier = qualifiers[i];
                if (qualifier.isDisabled)
                {
                    continue;
                }

                var score = qualifier.Score(context);
                if (score > max)
                {
                    max = score;
                    highRoller = qualifier;
                }
            }

            if (highRoller == null)
            {
                return defaultQualifier;
            }

            return highRoller;
        }
    }
}