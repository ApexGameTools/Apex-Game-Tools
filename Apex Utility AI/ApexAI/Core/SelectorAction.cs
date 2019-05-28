/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI
{
    using System;
    using Apex.AI.Serialization;
    using Apex.Serialization;
    using Apex.Utilities;

    /// <summary>
    /// An action that connects one selector to another.
    /// </summary>
    /// <seealso cref="Apex.AI.IConnectorAction" />
    /// <seealso cref="Apex.Serialization.IPrepareForSerialization" />
    /// <seealso cref="Apex.Serialization.IInitializeAfterDeserialization" />
    [FriendlyName("Continue"), Hidden]
    public class SelectorAction : IConnectorAction, IPrepareForSerialization, IInitializeAfterDeserialization
    {
        [ApexSerialization(hideInEditor = true)]
        private Guid _selectorId;
        private Selector _selector;

        internal SelectorAction()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectorAction"/> class.
        /// </summary>
        /// <param name="selector">The connected selector.</param>
        public SelectorAction(Selector selector)
        {
            Ensure.ArgumentNotNull(selector, "selector");

            _selector = selector;
        }

        /// <summary>
        /// Gets or sets the connected selector.
        /// </summary>
        /// <value>
        /// The selector.
        /// </value>
        public Selector selector
        {
            get { return _selector; }
            set { _selector = value; }
        }

        /// <summary>
        /// Executes the action.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute(IAIContext context)
        {
            /* NOOP */
        }

        public IAction Select(IAIContext context)
        {
            return _selector.Select(context);
        }

        /// <summary>
        /// Will be called by the engine just before the AI is serialized.
        /// </summary>
        void IPrepareForSerialization.Prepare()
        {
            if (_selector != null)
            {
                _selectorId = _selector.id;
            }
        }

        /// <summary>
        /// Will be called by the engine after deserialization of the AI is complete, allowing the implementing entity to initialize itself before the AI starts running.
        /// </summary>
        /// <param name="rootObject">The parent AI.</param>
        void IInitializeAfterDeserialization.Initialize(object rootObject)
        {
            var parentAI = (IUtilityAI)rootObject;
            _selector = parentAI.FindSelector(_selectorId);
        }
    }
}
