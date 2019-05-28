/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Components
{
    /// <summary>
    /// AI client interface. Controls the AI life cycle of an AI for a single unit.
    /// </summary>
    public interface IUtilityAIClient
    {
        /// <summary>
        /// Gets or sets the AI of the client.
        /// </summary>
        IUtilityAI ai
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the state if the client.
        /// </summary>
        UtilityAIClientState state
        {
            get;
        }

        /// <summary>
        /// Starts the AI and sets <see cref="state"/> to <see cref="UtilityAIClientState.Running"/>. Typically this will also register the client with the <see cref="AIManager"/>.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the AI for this client and sets <see cref="state"/> to <see cref="UtilityAIClientState.Stopped"/>. Typically this will also unregister it with the <see cref="AIManager"/>
        /// </summary>
        void Stop();

        /// <summary>
        /// Executes the AI. Typically this is called by whatever manager controls the AI execution cycle.
        /// </summary>
        void Execute();

        /// <summary>
        /// Pauses the AI and sets <see cref="state"/> to <see cref="UtilityAIClientState.Paused"/>, call <see cref="Resume"/> to resume.
        /// </summary>
        void Pause();

        /// <summary>
        /// Resumes the AI and sets <see cref="state"/> to <see cref="UtilityAIClientState.Running"/> if it was previously <see cref="Pause"/>d
        /// </summary>
        void Resume();
    }
}
