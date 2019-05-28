/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Visualization
{
    using System.Collections;
    using System.Collections.Generic;

    internal class CompositeQualifierVisualizer : QualifierVisualizer, ICompositeVisualizer
    {
        private List<IContextualScorer> _scorers;

        internal CompositeQualifierVisualizer(ICompositeScorer q, SelectorVisualizer parent)
            : base((IQualifier)q, parent)
        {
            _scorers = new List<IContextualScorer>(q.scorers.Count);
            foreach (var s in q.scorers)
            {
                _scorers.Add(new ScorerVisualizer(s, this));
            }
        }

        IList ICompositeVisualizer.children
        {
            get { return _scorers; }
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public override void Reset()
        {
            this.lastScore = null;
            this.breakPointHit = false;

            var scorerCount = _scorers.Count;
            for (int i = 0; i < scorerCount; i++)
            {
                ((ScorerVisualizer)_scorers[i]).Reset();
            }
        }

        /// <summary>
        /// Calculates a score using the supplied scorers given the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>
        /// The score.
        /// </returns>
        public override float Score(IAIContext context)
        {
            var score = ((ICompositeScorer)this.qualifier).Score(context, _scorers);
            this.lastScore = score;

            ICustomVisualizer customVisualizer;
            if (VisualizationManager.TryGetVisualizerFor(this.qualifier.GetType(), out customVisualizer))
            {
                customVisualizer.EntityUpdate(this.qualifier, context, this.parent.parent.id);
            }

            return score;
        }

        void ICompositeVisualizer.Add(object item)
        {
            var scorer = item as IContextualScorer;
            if (scorer == null)
            {
                return;
            }

            _scorers.Add(new ScorerVisualizer(scorer, this));
        }
    }
}
