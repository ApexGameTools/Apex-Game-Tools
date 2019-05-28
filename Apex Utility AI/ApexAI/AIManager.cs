/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI
{
    using System;
    using System.Collections.Generic;
    using Apex.AI.Components;
    using Apex.AI.Serialization;
    using Apex.AI.Visualization;
    using Apex.Serialization;
    using Apex.Utilities;
    using UnityEngine;

    /// <summary>
    /// Manages AIs and AI clients.
    /// </summary>
    public static class AIManager
    {
        /// <summary>
        /// The AI storage folder
        /// </summary>
        public const string StorageFolder = "ApexAIStorage";

        private static readonly object initLock = new object();
        private static Dictionary<Guid, AIData> _aiLookup;
        private static Dictionary<Guid, List<IUtilityAIClient>> _aiClients;

        /// <summary>
        /// Signature of delegate for resolving AI clients from a Game Object.
        /// </summary>
        /// <param name="host">The host game object for which to obtain the AI Client.</param>
        /// <param name="aiId">The AI ID.</param>
        /// <returns>The AI client matching the ID, or null if no match is found.</returns>
        public delegate IUtilityAIClient AIClientResolver(GameObject host, Guid aiId);

        /// <summary>
        /// Gets all registered clients.
        /// </summary>
        public static IEnumerable<IUtilityAIClient> allClients
        {
            get
            {
                if (_aiClients != null)
                {
                    foreach (var clientList in _aiClients.Values)
                    {
                        foreach (var client in clientList)
                        {
                            yield return client;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Delegate for resolving AI clients from a Game Object. The default will resolve using the <see cref="UtilityAIComponent"/>.
        /// </summary>
        public static AIClientResolver GetAIClient = (host, aiId) =>
        {
            var aiComponent = host.GetComponent<UtilityAIComponent>();
            if (aiComponent != null)
            {
                return aiComponent.GetClient(aiId);
            }

            return null;
        };

        /// <summary>
        /// Gets the list of clients for a given AI. Please note that this is a live list that should not be modified directly.
        /// </summary>
        /// <param name="aiId">The AI ID.</param>
        /// <returns>The list of clients for the specified AI</returns>
        public static IList<IUtilityAIClient> GetAIClients(Guid aiId)
        {
            if (_aiClients != null)
            {
                List<IUtilityAIClient> clients;
                if (_aiClients.TryGetValue(aiId, out clients))
                {
                    return clients;
                }
            }

            return Empty<IUtilityAIClient>.array;
        }

        /// <summary>
        /// Gets an AI by ID.
        /// </summary>
        /// <param name="id">The ID.</param>
        /// <returns>The AI with the specified ID, or null if no match is found.</returns>
        public static IUtilityAI GetAI(Guid id)
        {
            EnsureLookup(false);

            AIData aiData;
            if (!_aiLookup.TryGetValue(id, out aiData))
            {
                return null;
            }

            if (aiData.ai == null)
            {
                lock (initLock)
                {
                    if (aiData.ai == null)
                    {
                        ReadAndInit(aiData);
                    }
                }
            }

            return aiData.ai;
        }

        /// <summary>
        /// Executes the specified AI once.
        /// </summary>
        /// <param name="id">The AI ID.</param>
        /// <param name="context">The context.</param>
        /// <returns><c>true</c> if the AI was found and executed; otherwise <c>false</c>.</returns>
        public static bool ExecuteAI(Guid id, IAIContext context)
        {
            var ai = GetAI(id);
            if (ai == null)
            {
                return false;
            }

            return ai.ExecuteOnce(context);
        }

        /// <summary>
        /// Loads and initializes all AIs. This means that calling <see cref="GetAI"/> will never load AIs on demand and thus won't allocate memory.
        /// </summary>
        public static void EagerLoadAll()
        {
            EnsureLookup(true);
        }

        /// <summary>
        /// Registers the specified client.
        /// </summary>
        /// <param name="client">The client.</param>
        public static void Register(IUtilityAIClient client)
        {
            var id = client.ai.id;
            List<IUtilityAIClient> clients;

            if (_aiClients == null)
            {
                _aiClients = new Dictionary<Guid, List<IUtilityAIClient>>();
            }

            if (!_aiClients.TryGetValue(id, out clients))
            {
                clients = new List<IUtilityAIClient>(1);
                _aiClients.Add(id, clients);
            }

            clients.Add(client);

            if (Application.isEditor && VisualizationManager.isVisualizing && !(client.ai is UtilityAIVisualizer))
            {
                client.ai = new UtilityAIVisualizer(client.ai);
            }
        }

        /// <summary>
        /// Unregisters the specified client.
        /// </summary>
        /// <param name="client">The client.</param>
        public static void Unregister(IUtilityAIClient client)
        {
            if (_aiClients == null)
            {
                return;
            }

            var id = client.ai.id;
            List<IUtilityAIClient> clients;

            if (!_aiClients.TryGetValue(id, out clients))
            {
                return;
            }

            clients.Remove(client);

            if (Application.isEditor && VisualizationManager.isVisualizing && (client.ai is UtilityAIVisualizer))
            {
                ((UtilityAIVisualizer)client.ai).Reset();
            }
        }

        private static void ReadAndInit(AIData data)
        {
            var requiresInit = new List<IInitializeAfterDeserialization>();

            try
            {
                data.ai = SerializationMaster.Deserialize<UtilityAI>(data.storedData.configuration, requiresInit);
                data.ai.name = data.storedData.name;
            }
            catch (Exception e)
            {
                Debug.LogWarning(string.Format("Unable to load the AI: {0}. Additional details: {1}\n{2}", data.storedData.name, e.Message, e.StackTrace));
                return;
            }

            var initCount = requiresInit.Count;
            for (int i = 0; i < initCount; i++)
            {
                requiresInit[i].Initialize(data.ai);
            }
        }

        private static void EnsureLookup(bool init)
        {
            if (_aiLookup != null)
            {
                return;
            }

            lock (initLock)
            {
                if (_aiLookup != null)
                {
                    return;
                }

                _aiLookup = new Dictionary<Guid, AIData>();
            }

            var storedAIs = Resources.LoadAll<AIStorage>(StorageFolder);
            for (int i = 0; i < storedAIs.Length; i++)
            {
                var aiData = new AIData
                {
                    storedData = storedAIs[i]
                };

                _aiLookup.Add(new Guid(aiData.storedData.aiId), aiData);

                if (init)
                {
                    ReadAndInit(aiData);
                }
            }
        }

        private class AIData
        {
            public IUtilityAI ai;
            public AIStorage storedData;
        }
    }
}
