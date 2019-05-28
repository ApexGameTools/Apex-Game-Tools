/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI
{
    using System.Collections.Generic;
    using Apex.Serialization;

    /// <summary>
    /// A base class for Qualifiers that base their score on a number of child <see cref="IContextualScorer"/>s.
    /// </summary>
    /// <seealso cref="Apex.AI.IQualifier" />
    /// <seealso cref="Apex.AI.ICompositeScorer" />
    /// <seealso cref="Apex.AI.ICanClone" />
    public abstract class CompositeQualifier : IQualifier, ICompositeScorer, ICanClone
    {
        /// <summary>
        /// The scorers on which to base the final score.
        /// </summary>
        [ApexSerialization, MemberCategory(null, 10000)]
        protected List<IContextualScorer> _scorers;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeQualifier"/> class.
        /// </summary>
        protected CompositeQualifier()
        {
            _scorers = new List<IContextualScorer>();
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is disabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is disabled; otherwise, <c>false</c>.
        /// </value>
        [ApexSerialization(hideInEditor = true, defaultValue = false)]
        public bool isDisabled
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the associated action.
        /// </summary>
        /// <value>
        /// The action.
        /// </value>
        [ApexSerialization(hideInEditor = true)]
        public IAction action
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the scorers that this composite uses.
        /// </summary>
        /// <value>
        /// The scorers.
        /// </value>
        public IList<IContextualScorer> scorers
        {
            get { return _scorers; }
        }

        /// <summary>
        /// Calculates the score for this qualifier given the provided context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>
        /// The score.
        /// </returns>
        public float Score(IAIContext context)
        {
            return Score(context, _scorers);
        }

        /// <summary>
        /// Calculates a score using the supplied scorers given the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="scorers">The scorers.</param>
        /// <returns>
        /// The score.
        /// </returns>
        public abstract float Score(IAIContext context, IList<IContextualScorer> scorers);

        /// <summary>
        /// Clones or transfers settings from the other entity to itself.
        /// </summary>
        /// <param name="other">The other entity.</param>
        public virtual void CloneFrom(object other)
        {
            var source = other as CompositeQualifier;
            if (source == null)
            {
                return;
            }

            this.action = source.action;

            foreach (var s in source._scorers)
            {
                _scorers.Add(s);
            }
        }
    }
}
