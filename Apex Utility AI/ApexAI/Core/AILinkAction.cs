/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI
{
    using System;
    using Apex.Serialization;
    using UnityEngine;

    /// <summary>
    /// An AI action that executes another AI.
    /// </summary>
    /// <seealso cref="Apex.AI.IConnectorAction" />
    /// <seealso cref="Apex.Serialization.IInitializeAfterDeserialization" />
    [FriendlyName("AI Link"), Hidden]
    public sealed class AILinkAction : IConnectorAction, IInitializeAfterDeserialization
    {
        [ApexSerialization(hideInEditor = true)]
        private Guid _aiId;
        private ISelect _linkedAI;

        internal AILinkAction()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AILinkAction"/> class.
        /// </summary>
        /// <param name="aiId">The AI identifier.</param>
        public AILinkAction(Guid aiId)
        {
            _aiId = aiId;
        }

        /// <summary>
        /// Gets or sets the AI identifier.
        /// </summary>
        /// <value>
        /// The AI identifier.
        /// </value>
        public Guid aiId
        {
            get { return _aiId; }
            set { _aiId = value; }
        }

        internal IUtilityAI linkedAI
        {
            get { return _linkedAI as IUtilityAI; }
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
            return _linkedAI.Select(context);
        }

        /// <summary>
        /// Will be called by the engine after deserialization of the AI is complete, allowing the implementing entity to initialize itself before the AI starts running.
        /// </summary>
        /// <param name="rootObject">The parent AI.</param>
        void IInitializeAfterDeserialization.Initialize(object rootObject)
        {
            if (!Application.isPlaying)
            {
                return;
            }

            _linkedAI = AIManager.GetAI(_aiId);
            if (_linkedAI == null)
            {
                _linkedAI = new BrokenLink();
                
                Debug.LogWarning(string.Format("{0} : Failed to initialize a linked AI, the ID does not match an existing AI.", ((IUtilityAI)rootObject).name));
            }
        }

        private class BrokenLink : ISelect
        {
            public Guid id
            {
                get { return Guid.Empty; }
            }

            public IAction Select(IAIContext context)
            {
                return null;
            }
        }
    }
}
