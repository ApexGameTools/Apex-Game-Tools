/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Serialization
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Storage entity for AIs
    /// </summary>
    /// <seealso cref="UnityEngine.ScriptableObject" />
    /// <seealso cref="Apex.AI.IHaveFriendlyName" />
    public sealed class AIStorage : ScriptableObject, IHaveFriendlyName
    {
        /// <summary>
        /// The description
        /// </summary>
        [TextArea(1, 20)]
        public string description;

        /// <summary>
        /// The version of the AI with which this was last saved.
        /// </summary>
        [HideInInspector]
        public int version;

        /// <summary>
        /// The AI ID
        /// </summary>
        [HideInInspector]
        public string aiId;

        /// <summary>
        /// The AI configuration
        /// </summary>
        [HideInInspector]
        public string configuration;

        /// <summary>
        /// The AI editor configuration
        /// </summary>
        [HideInInspector]
        public string editorConfiguration;

        string IHaveFriendlyName.friendlyName
        {
            get { return this.name; }
        }

        string IHaveFriendlyName.description
        {
            get { return this.description; }
        }

        /// <summary>
        /// Creates an instance for the specified AI.
        /// </summary>
        /// <param name="aiId">The AI ID.</param>
        /// <param name="aiName">Name of the AI.</param>
        /// <returns></returns>
        public static AIStorage Create(string aiId, string aiName)
        {
            var s = ScriptableObject.CreateInstance<AIStorage>();
            s.name = aiName;
            s.aiId = aiId;

            return s;
        }
    }
}
