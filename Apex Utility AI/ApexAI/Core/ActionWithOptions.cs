/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI
{
    using System.Collections.Generic;
    using Apex.Serialization;

    /// <summary>
    /// Base class for an AI action that can select from a number of options to use in its execution.
    /// </summary>
    /// <typeparam name="TOption">The type of the option.</typeparam>
    /// <seealso cref="Apex.AI.IAction" />
    /// <seealso cref="Apex.AI.ICanClone" />
    public abstract class ActionWithOptions<TOption> : IAction, ICanClone
    {
        /// <summary>
        /// The child options scorers used to select the option(s) used in execution of the action.
        /// </summary>
        [ApexSerialization, MemberCategory(null, 10000)]
        protected List<IOptionScorer<TOption>> _scorers;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionWithOptions{TOption}"/> class.
        /// </summary>
        protected ActionWithOptions()
        {
            _scorers = new List<IOptionScorer<TOption>>();
        }

        /// <summary>
        /// Gets the child options scorers used to select the option(s) used in execution of the action.
        /// </summary>
        /// <value>
        /// The scorers.
        /// </value>
        public IList<IOptionScorer<TOption>> scorers
        {
            get { return _scorers; }
        }

        /// <summary>
        /// Gets the best option, i.e. the option that was given the highest combined score by the <see cref="scorers"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="options">The options from which to find the best.</param>
        /// <returns>The best option.</returns>
        public TOption GetBest(IAIContext context, IList<TOption> options)
        {
            TOption best = default(TOption);
            float maxScore = float.MinValue;

            var ocount = options.Count;
            for (int i = 0; i < ocount; i++)
            {
                var option = options[i];

                float accumulator = 0f;
                var scount = _scorers.Count;
                for (int j = 0; j < scount; j++)
                {
                    var scorer = _scorers[j];
                    if (scorer.isDisabled)
                    {
                        continue;
                    }

                    accumulator += scorer.Score(context, option);
                }

                if (accumulator > maxScore)
                {
                    best = option;
                    maxScore = accumulator;
                }
            }

            return best;
        }

        /// <summary>
        /// Gets all options with the score they received from the <see cref="scorers"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="options">The options.</param>
        /// <param name="optionsBuffer">The buffer which is populated with the scored options.</param>
        public void GetAllScores(IAIContext context, IList<TOption> options, IList<ScoredOption<TOption>> optionsBuffer)
        {
            var count = options.Count;
            for (int i = 0; i < count; i++)
            {
                var option = options[i];

                var score = 0f;
                var scount = _scorers.Count;
                for (int j = 0; j < scount; j++)
                {
                    var scorer = _scorers[j];
                    if (scorer.isDisabled)
                    {
                        continue;
                    }

                    score += scorer.Score(context, option);
                }

                optionsBuffer.Add(new ScoredOption<TOption>(option, score));
            }
        }

        /// <summary>
        /// Executes the action.
        /// </summary>
        /// <param name="context">The context.</param>
        public abstract void Execute(IAIContext context);

        /// <summary>
        /// Clones or transfers settings from the other entity to itself.
        /// </summary>
        /// <param name="other">The other entity.</param>
        public virtual void CloneFrom(object other)
        {
            var source = other as ActionWithOptions<TOption>;
            if (source == null)
            {
                return;
            }

            foreach (var o in source._scorers)
            {
                _scorers.Add(o);
            }
        }
    }
}
