/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Components
{
    using System;
    using Apex.Utilities;

    /// <summary>
    /// Default base class for AI clients.
    /// </summary>
    public abstract class UtilityAIClient : IUtilityAIClient
    {
        private IUtilityAI _ai;
        private IContextProvider _contextProvider;
        private IRequireTermination _activeAction;

        /// <summary>
        /// Initializes a new instance of the <see cref="UtilityAIClient"/> class.
        /// </summary>
        /// <param name="aiId">The AI id.</param>
        /// <param name="contextProvider">The context provider.</param>
        /// <exception cref="System.ArgumentException">Unable to load associated AI.;aiId</exception>
        protected UtilityAIClient(Guid aiId, IContextProvider contextProvider)
        {
            Ensure.ArgumentNotNull(contextProvider, "contextProvider");

            _ai = AIManager.GetAI(aiId);
            if (_ai == null)
            {
                throw new ArgumentException("Unable to load associated AI.", "aiId");
            }

            _contextProvider = contextProvider;
            this.state = UtilityAIClientState.Stopped;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UtilityAIClient"/> class.
        /// </summary>
        /// <param name="ai">The AI.</param>
        /// <param name="contextProvider">The context provider.</param>
        protected UtilityAIClient(IUtilityAI ai, IContextProvider contextProvider)
        {
            Ensure.ArgumentNotNull(ai, "ai");
            Ensure.ArgumentNotNull(contextProvider, "contextProvider");

            _ai = ai;
            _contextProvider = contextProvider;
            this.state = UtilityAIClientState.Stopped;
        }

        /// <summary>
        /// Gets the AI of the client.
        /// </summary>
        public IUtilityAI ai
        {
            get { return _ai; }
            set { _ai = value; }
        }

        /// <summary>
        /// Gets the state if the client.
        /// </summary>
        public UtilityAIClientState state
        {
            get;
            protected set;
        }

        /// <summary>
        /// Starts the AI and sets <see cref="state"/> to <see cref="UtilityAIClientState.Running"/>. This also registers the client with the <see cref="AIManager"/>.
        /// Deriving classes must implement <see cref="OnStart"/> to do the actual starting.
        /// </summary>
        public void Start()
        {
            if (this.state != UtilityAIClientState.Stopped)
            {
                return;
            }

            AIManager.Register(this);
            this.state = UtilityAIClientState.Running;

            OnStart();
        }

        /// <summary>
        /// Stops the AI for this client and sets <see cref="state"/> to <see cref="UtilityAIClientState.Stopped"/>. This also unregisters the client with the <see cref="AIManager" />.
        /// Deriving classes must implement <see cref="OnStop"/> to do the actual stopping.
        /// </summary>
        public void Stop()
        {
            if (this.state == UtilityAIClientState.Stopped)
            {
                return;
            }

            AIManager.Unregister(this);
            this.state = UtilityAIClientState.Stopped;

            OnStop();
        }

        /// <summary>
        /// Pauses the AI and sets <see cref="state"/> to <see cref="UtilityAIClientState.Paused"/>, call <see cref="Resume"/> to resume.
        /// Deriving classes must implement <see cref="OnPause"/> to do the actual pausing.
        /// </summary>
        public void Pause()
        {
            if (this.state != UtilityAIClientState.Running)
            {
                return;
            }

            this.state = UtilityAIClientState.Paused;
            OnPause();
        }

        /// <summary>
        /// Resumes the AI and sets <see cref="state"/> to <see cref="UtilityAIClientState.Running"/> if it was previously <see cref="Pause" />d.
        /// Deriving classes must implement <see cref="OnResume"/> to do the actual resuming.
        /// </summary>
        public void Resume()
        {
            if (this.state != UtilityAIClientState.Paused)
            {
                return;
            }

            this.state = UtilityAIClientState.Running;
            OnResume();
        }

        /// <summary>
        /// Executes the AI. Typically this is called by whatever manager controls the AI execution cycle.
        /// </summary>
        public void Execute()
        {
            IAIContext context = _contextProvider.GetContext(_ai.id);

            var action = _ai.Select(context);

            bool finalActionFound = false;

            while (!finalActionFound)
            {
                //While we could treat all connectors the same, most connectors will not have anything to execute, so this way we save the call to Execute.
                var composite = action as ICompositeAction;
                if (composite == null)
                {
                    var connector = action as IConnectorAction;
                    if (connector == null)
                    {
                        finalActionFound = true;
                    }
                    else
                    {
                        action = connector.Select(context);
                    }
                }
                else
                {
                    //For composites that also connect, we execute the child actions before moving on.
                    //So action is executed and then reassigned to the selected action if one exists.
                    if (composite.isConnector)
                    {
                        action.Execute(context);
                        action = composite.Select(context);
                    }
                    else
                    {
                        finalActionFound = true;
                    }
                }
            }

            if (_activeAction != null && !object.ReferenceEquals(_activeAction, action))
            {
                _activeAction.Terminate(context);
                _activeAction = action as IRequireTermination;
            }
            else if (_activeAction == null)
            {
                _activeAction = action as IRequireTermination;
            }

            if (action != null)
            {
                action.Execute(context);
            }
        }

        /// <summary>
        /// Called after <see cref="Start"/>. Deriving classes should implement additional start-up logic here.
        /// </summary>
        protected abstract void OnStart();

        /// <summary>
        /// Called after <see cref="Stop"/>. Deriving classes should implement additional tear-down logic here.
        /// </summary>
        protected abstract void OnStop();

        /// <summary>
        /// Called after <see cref="Pause"/>. Deriving classes should implement additional pause logic here.
        /// </summary>
        protected abstract void OnPause();

        /// <summary>
        /// Called after <see cref="Resume"/>. Deriving classes should implement additional resume logic here.
        /// </summary>
        protected abstract void OnResume();
    }
}
