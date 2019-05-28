/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI
{
    using System;
    using System.Collections.Generic;
    using Apex.Serialization;
    using Apex.Utilities;

    /// <summary>
    /// The Utility AI root entity.
    /// </summary>
    /// <seealso cref="Apex.AI.IUtilityAI" />
    /// <seealso cref="Apex.Serialization.IPrepareForSerialization" />
    /// <seealso cref="Apex.Serialization.IInitializeAfterDeserialization" />
    public sealed class UtilityAI : IUtilityAI, IPrepareForSerialization, IInitializeAfterDeserialization
    {
        [ApexSerialization(hideInEditor = true)]
        private Guid _rootSelectorId;

        [ApexSerialization(hideInEditor = true)]
        private Guid _id;

        [ApexSerialization(hideInEditor = true)]
        private List<Selector> _selectors;

        private Selector _rootSelector;

        /// <summary>
        /// Initializes a new instance of the <see cref="UtilityAI"/> class.
        /// </summary>
        public UtilityAI()
        {
            _id = Guid.NewGuid();
            _selectors = new List<Selector>(1);
        }

        /// <summary>
        /// Gets the ID of the AI.
        /// </summary>
        /// <value>
        /// The ID.
        /// </value>
        public Guid id
        {
            get { return _id; }
        }

        /// <summary>
        /// Gets or sets the name of the AI.
        /// </summary>
        /// <value>
        /// The name of the AI
        /// </value>
        public string name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the root selector of the AI.
        /// </summary>
        /// <value>
        /// The root selector of the AI.
        /// </value>
        public Selector rootSelector
        {
            get { return _rootSelector; }
            set { _rootSelector = value; }
        }

        /// <summary>
        /// Gets the number of selectors in the AI.
        /// </summary>
        /// <value>
        /// The number of selectors in the AI.
        /// </value>
        public int selectorCount
        {
            get { return _selectors.Count; }
        }

        /// <summary>
        /// Gets the <see cref="Selector"/> with the specified index.
        /// </summary>
        /// <value>
        /// The <see cref="Selector"/>.
        /// </value>
        /// <param name="idx">The index.</param>
        /// <returns></returns>
        public Selector this[int idx]
        {
            get { return _selectors[idx]; }
        }

        /// <summary>
        /// Adds the selector.
        /// </summary>
        /// <param name="s">The selector.</param>
        public void AddSelector(Selector s)
        {
            Ensure.ArgumentNotNull(s, "selector cannot be null");
            _selectors.Add(s);
        }

        /// <summary>
        /// Removes the selector.
        /// </summary>
        /// <param name="s">The selector.</param>
        /// <exception cref="ArgumentException">The root selector cannot be removed!</exception>
        public void RemoveSelector(Selector s)
        {
            if (s == _rootSelector)
            {
                throw new ArgumentException("The root selector cannot be removed!");
            }

            _selectors.Remove(s);

            var selectorCount = _selectors.Count;
            for (int i = 0; i < selectorCount; i++)
            {
                var qualifiers = _selectors[i].qualifiers;
                var qualifierCount = qualifiers.Count;
                for (int j = 0; j < qualifierCount; j++)
                {
                    var action = qualifiers[j].action as SelectorAction;
                    if (action == null)
                    {
                        var ca = qualifiers[j].action as CompositeAction;
                        if (ca != null)
                        {
                            action = ca.connectorAction as SelectorAction;
                        }
                    }

                    if (action != null && object.ReferenceEquals(action.selector, s))
                    {
                        qualifiers[j].action = null;
                    }
                }
            }
        }

        /// <summary>
        /// Replaces a selector with another selector.
        /// </summary>
        /// <param name="current">The current selector.</param>
        /// <param name="replacement">The replacement.</param>
        /// <returns>
        ///   <c>true</c> if the replacement was done; otherwise <c>false</c>
        /// </returns>
        public bool ReplaceSelector(Selector current, Selector replacement)
        {
            Ensure.ArgumentNotNull(current, "current cannot be null");
            Ensure.ArgumentNotNull(replacement, "replacement cannot be null");

            int idx = _selectors.IndexOf(current);
            if (idx >= 0)
            {
                _selectors[idx] = replacement;

                if (_rootSelector == current)
                {
                    _rootSelector = replacement;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Finds the selector with the specified ID.
        /// </summary>
        /// <param name="id">The ID.</param>
        /// <returns>
        /// The selector or <c>null</c> if not found.
        /// </returns>
        public Selector FindSelector(Guid id)
        {
            var selectorCount = _selectors.Count;
            for (int i = 0; i < selectorCount; i++)
            {
                if (_selectors[i].id.Equals(id))
                {
                    return _selectors[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Selects the action for execution, given the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>
        /// The action to execute.
        /// </returns>
        public IAction Select(IAIContext context)
        {
            return _rootSelector.Select(context);
        }

        /// <summary>
        /// Regenerates the IDs of this AI.
        /// </summary>
        public void RegenerateIds()
        {
            _id = Guid.NewGuid();
            var count = _selectors.Count;
            for (int i = 0; i < count; i++)
            {
                _selectors[i].RegenerateId();
            }
        }

        /// <summary>
        /// Will be called by the engine just before the AI is serialized.
        /// </summary>
        void IPrepareForSerialization.Prepare()
        {
            _rootSelectorId = _rootSelector.id;
        }

        /// <summary>
        /// Will be called by the engine after deserialization of the AI is complete, allowing the implementing entity to initialize itself before the AI starts running.
        /// </summary>
        /// <param name="rootObject">The parent AI.</param>
        void IInitializeAfterDeserialization.Initialize(object rootObject)
        {
            _rootSelector = FindSelector(_rootSelectorId);
        }
    }
}
