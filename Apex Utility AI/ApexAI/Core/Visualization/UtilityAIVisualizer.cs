/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Visualization
{
    using System;
    using System.Collections.Generic;

    internal sealed class UtilityAIVisualizer : IUtilityAI
    {
        private IUtilityAI _ai;
        private List<Action> _postExecute;
        private SelectorVisualizer _visualizerRootSelector;
        private List<SelectorVisualizer> _selectorVisualizers;
        private List<UtilityAIVisualizer> _linkedAIs = new List<UtilityAIVisualizer>();

        internal UtilityAIVisualizer(IUtilityAI ai)
        {
            _ai = ai;

            var selectorCount = ai.selectorCount;
            _selectorVisualizers = new List<SelectorVisualizer>(ai.selectorCount);
            for (int i = 0; i < selectorCount; i++)
            {
                var selector = _ai[i];
                var selectorVisualizer = new SelectorVisualizer(selector, this);
                _selectorVisualizers.Add(selectorVisualizer);

                if (object.ReferenceEquals(selector, _ai.rootSelector))
                {
                    _visualizerRootSelector = selectorVisualizer;
                }
            }

            for (int i = 0; i < selectorCount; i++)
            {
                _selectorVisualizers[i].Init();
            }

            _postExecute = new List<Action>(1);
        }

        public Guid id
        {
            get { return _ai.id; }
        }

        public string name
        {
            get { return _ai.name; }
            set { _ai.name = value; }
        }

        public IUtilityAI ai
        {
            get { return _ai; }
        }

        internal List<UtilityAIVisualizer> linkedAIs
        {
            get { return _linkedAIs; }
        }

        public Selector rootSelector
        {
            get { return _visualizerRootSelector; }
            set { throw new NotSupportedException("Cannot edit a Visualizer."); }
        }

        public int selectorCount
        {
            get { return _selectorVisualizers.Count; }
        }

        public Selector this[int idx]
        {
            get { return _selectorVisualizers[idx]; }
        }

        internal void PostExecute()
        {
            var count = _postExecute.Count;
            for (int i = 0; i < count; i++)
            {
                _postExecute[i]();
            }
        }

        internal void Hook(Action postExecute)
        {
            if (!_postExecute.Contains(postExecute))
            {
                _postExecute.Add(postExecute);
            }
        }

        internal void Unhook(Action postExecute)
        {
            _postExecute.Remove(postExecute);
        }

        public Selector FindSelector(Guid id)
        {
            var selectorCount = _selectorVisualizers.Count;
            for (int i = 0; i < selectorCount; i++)
            {
                if (_selectorVisualizers[i].id.Equals(id))
                {
                    return _selectorVisualizers[i];
                }
            }

            return null;
        }

        internal IQualifierVisualizer FindQualifierVisualizer(IQualifier target)
        {
            var selectorCount = _selectorVisualizers.Count;
            for (int i = 0; i < selectorCount; i++)
            {
                var s = _selectorVisualizers[i];
                var qualiferCount = s.qualifiers.Count;
                for (int j = 0; j < qualiferCount; j++)
                {
                    var q = (IQualifierVisualizer)s.qualifiers[j];
                    if (ReferenceEquals(q.qualifier, target))
                    {
                        return q;
                    }
                }
            }

            return null;
        }

        internal ActionVisualizer FindActionVisualizer(IAction target)
        {
            ActionVisualizer result = null;

            var selectorCount = _selectorVisualizers.Count;
            for (int i = 0; i < selectorCount; i++)
            {
                var s = _selectorVisualizers[i];
                var qualiferCount = s.qualifiers.Count;
                for (int j = 0; j < qualiferCount; j++)
                {
                    var q = (IQualifierVisualizer)s.qualifiers[j];

                    if (TryFindActionVisualizer(q.action, target, out result))
                    {
                        return result;
                    }
                }

                if (TryFindActionVisualizer(s.defaultQualifier.action, target, out result))
                {
                    return result;
                }
            }

            return null;
        }

        private static bool TryFindActionVisualizer(IAction source, IAction target, out ActionVisualizer result)
        {
            result = null;

            var av = source as ActionVisualizer;
            if (av == null)
            {
                return false;
            }

            if (ReferenceEquals(av.action, target))
            {
                result = av;
                return true;
            }

            var cav = av as ICompositeVisualizer;
            if (cav == null)
            {
                return false;
            }

            int count = cav.children.Count;
            for (int i = 0; i < count; i++)
            {
                if (TryFindActionVisualizer(cav.children[i] as IAction, target, out result))
                {
                    return true;
                }
            }

            return false;
        }

        public IAction Select(IAIContext context)
        {
            Reset();

            var action = _visualizerRootSelector.Select(context);
            if (action == null)
            {
                PostExecute();
            }

            return action;
        }

        internal void ClearBreakpoints()
        {
            var selectorCount = _selectorVisualizers.Count;
            for (int i = 0; i < selectorCount; i++)
            {
                _selectorVisualizers[i].ClearBreakpoints();
            }
        }

        void IUtilityAI.AddSelector(Selector s)
        {
            throw new NotSupportedException("Cannot alter AI during visualization.");
        }

        void IUtilityAI.RemoveSelector(Selector s)
        {
            throw new NotSupportedException("Cannot alter AI during visualization.");
        }

        bool IUtilityAI.ReplaceSelector(Selector current, Selector replacement)
        {
            throw new NotSupportedException("Cannot alter AI during visualization.");
        }

        void IUtilityAI.RegenerateIds()
        {
            throw new NotSupportedException("Cannot alter AI during visualization.");
        }

        internal void Reset()
        {
            var selectorCount = _selectorVisualizers.Count;
            for (int i = 0; i < selectorCount; i++)
            {
                _selectorVisualizers[i].Reset();
            }

            var linkedCount = _linkedAIs.Count;
            for (int i = 0; i < linkedCount; i++)
            {
                _linkedAIs[i].Reset();
                _linkedAIs[i].PostExecute();
            }
        }
    }
}
