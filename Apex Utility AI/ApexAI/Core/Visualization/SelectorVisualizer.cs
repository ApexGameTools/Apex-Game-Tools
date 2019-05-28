/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Visualization
{
    using System;
    using System.Collections.Generic;

    internal sealed class SelectorVisualizer : Selector, IVisualizedObject
    {
        private Selector _selector;
        private UtilityAIVisualizer _parent;

        internal SelectorVisualizer(Selector s, UtilityAIVisualizer parent)
        {
            _selector = s;
            _parent = parent;

            var sourceQualifiers = _selector.qualifiers;
            var qualifierCount = sourceQualifiers.Count;
            for (int i = 0; i < qualifierCount; i++)
            {
                var q = sourceQualifiers[i];
                if (q is ICompositeScorer)
                {
                    this.qualifiers.Add(new CompositeQualifierVisualizer((ICompositeScorer)q, this));
                }
                else
                {
                    this.qualifiers.Add(new QualifierVisualizer(q, this));
                }
            }

            this.defaultQualifier = new DefaultQualifierVisualizer(_selector.defaultQualifier, this);
        }

        internal UtilityAIVisualizer parent
        {
            get { return _parent; }
        }

        internal new Guid id
        {
            get { return _selector.id; }
        }

        internal Selector selector
        {
            get { return _selector; }
        }

        internal IQualifier lastSelectedQualifier
        {
            get;
            private set;
        }

        object IVisualizedObject.target
        {
            get { return _selector; }
        }

        internal void ClearBreakpoints()
        {
            var qualifierCount = this.qualifiers.Count;
            for (int i = 0; i < qualifierCount; i++)
            {
                ((IQualifierVisualizer)this.qualifiers[i]).isBreakPoint = false;
            }
        }

        internal void Init()
        {
            var qualifierCount = this.qualifiers.Count;
            for (int i = 0; i < qualifierCount; i++)
            {
                ((IQualifierVisualizer)this.qualifiers[i]).Init();
            }

            ((IQualifierVisualizer)this.defaultQualifier).Init();
        }

        internal void Reset()
        {
            this.lastSelectedQualifier = null;
            var qualifierCount = this.qualifiers.Count;
            for (int i = 0; i < qualifierCount; i++)
            {
                ((IQualifierVisualizer)this.qualifiers[i]).Reset();
            }

            ((IQualifierVisualizer)this.defaultQualifier).Reset();
        }

        /// <summary>
        /// Selects the action for execution, given the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="qualifiers">The qualifiers from which to find the action.</param>
        /// <param name="defaultQualifier">The default qualifier.</param>
        /// <returns>
        /// The qualifier whose action should be chosen.
        /// </returns>
        public override IQualifier Select(IAIContext context, IList<IQualifier> qualifiers, IDefaultQualifier defaultQualifier)
        {
            this.lastSelectedQualifier = _selector.Select(context, qualifiers, defaultQualifier);
            var qv = (IQualifierVisualizer)this.lastSelectedQualifier;
            if (qv.isBreakPoint)
            {
                if (qv.breakpointCondition != null)
                {
                    qv.breakPointHit = qv.breakpointCondition.Evaluate(qv.lastScore);
                }
                else
                {
                    qv.breakPointHit = true;
                }
            }

            return this.lastSelectedQualifier;
        }
    }
}
