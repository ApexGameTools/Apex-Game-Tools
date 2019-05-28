/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI
{
    using System;
    using System.Collections.Generic;
    using Apex.Serialization;

    /// <summary>
    /// An AI action that is comprised of one or more other actions. Note that <see cref="IRequireTermination"/> actions will not be terminated if part of a Composite, instead used the <see cref="TerminableCompositeAction"/>.
    /// </summary>
    /// <seealso cref="Apex.AI.IConnectorAction" />
    /// <seealso cref="Apex.AI.ICanClone" />
    [FriendlyName("Composite Action", "An action comprised of one or more child actions, which are executed in order")]
    public class CompositeAction : ICompositeAction, ICanClone
    {
        /// <summary>
        /// The child actions
        /// </summary>
        [ApexSerialization, MemberCategory(null, 10000)]
        protected List<IAction> _actions = new List<IAction>(2);

        [ApexSerialization(defaultValue = null, hideInEditor = true)]
        private IConnectorAction _connectorAction;

        /// <summary>
        /// Gets the child actions.
        /// </summary>
        /// <value>
        /// The actions.
        /// </value>
        public IList<IAction> actions
        {
            get { return _actions; }
        }

        /// <summary>
        /// Gets or sets the connector action, e.g. SelectorAction if one exists.
        /// </summary>
        internal IConnectorAction connectorAction
        {
            get { return _connectorAction; }
            set { _connectorAction = value; }
        }

        bool ICompositeAction.isConnector
        {
            get { return _connectorAction != null; }
        }

        /// <summary>
        /// Executes the action, which means it executes each child in sequence.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute(IAIContext context)
        {
            var count = _actions.Count;
            for (int i = 0; i < count; i++)
            {
                _actions[i].Execute(context);
            }
        }

        public IAction Select(IAIContext context)
        {
            if (_connectorAction == null)
            {
                return null;
            }

            return _connectorAction.Select(context);
        }

        /// <summary>
        /// Clones or transfers settings from the other entity to itself.
        /// </summary>
        /// <param name="other">The other entity.</param>
        public void CloneFrom(object other)
        {
            var source = other as IAction;
            if (source == null)
            {
                return;
            }

            var csource = other as CompositeAction;
            if (csource != null)
            {
                this.actions.AddRange(csource.actions);
                this.connectorAction = csource.connectorAction;
            }
            else if (source is IConnectorAction)
            {
                this.connectorAction = (IConnectorAction)source;
            }
            else
            {
                this.actions.Add(source);
            }
        }
    }
}
