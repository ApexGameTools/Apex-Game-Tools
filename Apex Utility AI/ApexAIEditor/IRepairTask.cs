/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Editor
{
    using System;
    using Apex.Serialization;

    /// <summary>
    /// Interface that represents a custom repair task for the AI.
    /// </summary>
    public interface IRepairTask
    {
        /// <summary>
        /// Gets the version threshold, meaning the last version of an AI this should be applied to.
        /// The version refers to the Apex AI version the AI was last saved with.
        /// </summary>
        Version versionThreshold { get; }

        /// <summary>
        /// Gets a value indicating whether this repairer also repair editor configurations. If <c>false</c> <see cref="Repair"/> will have null passed to its second argument.
        /// </summary>
        bool repairsEditorConfiguration { get; }

        /// <summary>
        /// Repairs the specified AI configuration.
        /// </summary>
        /// <param name="aiConfig">The configuration for the actual AI.</param>
        /// <param name="aiEditorConfig">The editor configuration for the AI.</param>
        bool Repair(StageElement aiConfig, StageElement aiEditorConfig);
    }
}
