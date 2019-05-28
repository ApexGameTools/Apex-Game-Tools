/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Visualization
{
    internal class ScorerVisualizer : IContextualScorer
    {
        private IContextualScorer _scorer;
        private CompositeQualifierVisualizer _parent;

        internal ScorerVisualizer(IContextualScorer scorer, CompositeQualifierVisualizer parent)
        {
            _scorer = scorer;
            _parent = parent;
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether this instance is disabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is disabled; otherwise, <c>false</c>.
        /// </value>
        public bool isDisabled
        {
            get { return _scorer.isDisabled; }
            set { _scorer.isDisabled = value; }
        }

        internal CompositeQualifierVisualizer parent
        {
            get { return _parent; }
        }

        internal IContextualScorer scorer
        {
            get { return _scorer; }
        }

        internal string lastScore
        {
            get;
            private set;
        }

        internal void Reset()
        {
            this.lastScore = "-";
        }

        /// <summary>
        /// Calculates a score given the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>
        /// The score.
        /// </returns>
        public float Score(IAIContext context)
        {
            var score = _scorer.Score(context);
            this.lastScore = score.ToString("f0");

            ICustomVisualizer customVisualizer;
            if (VisualizationManager.TryGetVisualizerFor(_scorer.GetType(), out customVisualizer))
            {
                customVisualizer.EntityUpdate(_scorer, context, _parent.parent.parent.id);
            }

            return score;
        }
    }
}
