/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Visualization
{
    using System;

    internal class QualifierVisualizer : IQualifierVisualizer, IVisualizedObject
    {
        private IQualifier _qualifier;
        private ActionVisualizer _action;
        private SelectorVisualizer _parent;

        internal QualifierVisualizer(IQualifier q, SelectorVisualizer parent)
        {
            _qualifier = q;
            _parent = parent;

            var selectorAction = q.action as SelectorAction;
            var linkAction = q.action as AILinkAction;
            var compAction = q.action as CompositeAction;
            var irt = q.action as IRequireTermination;
            if (selectorAction != null)
            {
                _action = new SelectorActionVisualizer(selectorAction, this);
            }
            else if (linkAction != null)
            {
                _action = new AILinkActionVisualizer(linkAction, this);
            }
            else if (compAction != null)
            {
                _action = new CompositeActionVisualizer(compAction, this);
            }
            else if (irt != null)
            {
                _action = new ActionRequiresTerminationVisualizer(q.action, this);
            }
            else if (q.action != null)
            {
                _action = new ActionVisualizer(q.action, this);
            }
        }

        /// <summary>
        /// Gets the parent.
        /// </summary>
        /// <value>
        /// The parent.
        /// </value>
        public SelectorVisualizer parent
        {
            get { return _parent; }
        }

        /// <summary>
        /// Gets the qualifier.
        /// </summary>
        /// <value>
        /// The qualifier.
        /// </value>
        public IQualifier qualifier
        {
            get { return _qualifier; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is disabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is disabled; otherwise, <c>false</c>.
        /// </value>
        public bool isDisabled
        {
            get { return _qualifier.isDisabled; }
            set { _qualifier.isDisabled = value; }
        }

        /// <summary>
        /// Gets or sets the associated action.
        /// </summary>
        /// <value>
        /// The action.
        /// </value>
        /// <exception cref="NotSupportedException"></exception>
        public IAction action
        {
            get { return _action; }
            set { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets a value indicating whether the qualifier is the current high scorer.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is high scorer; otherwise, <c>false</c>.
        /// </value>
        public bool isHighScorer
        {
            get { return object.ReferenceEquals(_parent.lastSelectedQualifier, this); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this qualifier has a break point set.
        /// </summary>
        /// <value>
        /// <c>true</c> if this qualifier has a break point set; otherwise, <c>false</c>.
        /// </value>
        public bool isBreakPoint
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this qualifier's break point was hit.
        /// </summary>
        /// <value>
        /// <c>true</c> if this qualifier's break point was hit; otherwise, <c>false</c>.
        /// </value>
        public bool breakPointHit
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the breakpoint condition.
        /// </summary>
        /// <value>
        /// The breakpoint condition.
        /// </value>
        public BreakpointCondition breakpointCondition
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the last score.
        /// </summary>
        /// <value>
        /// The last score.
        /// </value>
        public float? lastScore
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the target object of this visualizer, i.e. the visualized object.
        /// </summary>
        /// <value>
        /// The target.
        /// </value>
        object IVisualizedObject.target
        {
            get { return _qualifier; }
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        void IQualifierVisualizer.Init()
        {
            if (_action != null)
            {
                _action.Init();
            }
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public virtual void Reset()
        {
            this.lastScore = null;
            this.breakPointHit = false;
        }

        /// <summary>
        /// Calculates the score for this qualifier given the provided context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>
        /// The score.
        /// </returns>
        public virtual float Score(IAIContext context)
        {
            var score = _qualifier.Score(context);
            this.lastScore = score;

            ICustomVisualizer customVisualizer;
            if (VisualizationManager.TryGetVisualizerFor(_qualifier.GetType(), out customVisualizer))
            {
                customVisualizer.EntityUpdate(_qualifier, context, _parent.parent.id);
            }

            return score;
        }
    }
}
