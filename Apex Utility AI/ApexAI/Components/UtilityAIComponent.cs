/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Components
{
    using System;
    using Apex.Utilities;
    using UnityEngine;

    /// <summary>
    /// Component used to attach AIs to an entity.
    /// </summary>
    [AddComponentMenu("")] /* Added via quick start */
    [ApexComponent("AI")]
    public class UtilityAIComponent : ExtendedMonoBehaviour
    {
        /// <summary>
        /// The ai configurations
        /// </summary>
        [SerializeField, HideInInspector]
        internal UtilityAIConfig[] aiConfigs;

        private IUtilityAIClient[] _clients;
        private int _usedClients;

        internal event Action<IUtilityAIClient> OnNewAI;

        /// <summary>
        /// Gets all the AI clients.
        /// </summary>
        public IUtilityAIClient[] clients
        {
            get
            {
                if (_clients == null)
                {
                    var contextProvider = this.As<IContextProvider>();
                    if (contextProvider == null)
                    {
                        Debug.LogWarning(this.gameObject.name + ": No AI context provider was found.");
                        _clients = Empty<IUtilityAIClient>.array;
                        _usedClients = 0;
                        return _clients;
                    }

                    if (aiConfigs != null)
                    {
                        _clients = new IUtilityAIClient[aiConfigs.Length];
                        for (int i = 0; i < aiConfigs.Length; i++)
                        {
                            var aiCfg = aiConfigs[i];
                            if (aiCfg == null)
                            {
                                continue;
                            }

                            var ai = AIManager.GetAI(new Guid(aiCfg.aiId));
                            if (ai == null)
                            {
                                Debug.LogWarning(this.gameObject.name + ": Unable to load AI, no AI with the specified ID exists.");
                                _clients[i] = null;
                            }
                            else
                            {
                                _clients[i] = new LoadBalancedUtilityAIClient(ai, contextProvider, aiCfg.intervalMin, aiCfg.intervalMax, aiCfg.startDelayMin, aiCfg.startDelayMax);
                                _usedClients++;
                            }
                        }
                    }
                    else
                    {
                        _clients = Empty<IUtilityAIClient>.array;
                        _usedClients = 0;
                    }
                }

                return _clients;
            }
        }

        /// <summary>
        /// Gets the actual AI client for a specific AI.
        /// </summary>
        public IUtilityAIClient GetClient(Guid aiId)
        {
            var c = this.clients;
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] != null && c[i].ai.id == aiId)
                {
                    return c[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Adds a <see cref="LoadBalancedUtilityAIClient"/> to the list of clients maintained by this <see cref="UtilityAIComponent"/>.
        /// </summary>
        /// <param name="aiId">The ai identifier.</param>
        /// <returns></returns>
        public int AddClient(string aiId)
        {
            return AddClient(new Guid(aiId));
        }

        /// <summary>
        /// Adds a <see cref="LoadBalancedUtilityAIClient"/> to the list of clients maintained by this <see cref="UtilityAIComponent"/>.
        /// </summary>
        /// <param name="aiId">The ai identifier.</param>
        /// <returns></returns>
        public int AddClient(Guid aiId)
        {
            var contextProvider = this.As<IContextProvider>();
            if (contextProvider == null)
            {
                Debug.LogWarning(this.gameObject.name + ": No AI context provider was found.");
                return -1;
            }

            return AddClient(aiId, contextProvider);
        }

        /// <summary>
        /// Adds a <see cref="LoadBalancedUtilityAIClient"/> to the list of clients maintained by this <see cref="UtilityAIComponent"/>.
        /// </summary>
        /// <param name="aiId">The ai identifier.</param>
        /// <param name="contextProvider">The context provider.</param>
        /// <returns></returns>
        public int AddClient(Guid aiId, IContextProvider contextProvider)
        {
            return AddClient(aiId, contextProvider, 1f, 1f, 0f, 0f);
        }

        /// <summary>
        /// Adds a <see cref="LoadBalancedUtilityAIClient"/> to the list of clients maintained by this <see cref="UtilityAIComponent"/>.
        /// </summary>
        /// <param name="aiId">The ai identifier.</param>
        /// <param name="contextProvider">The context provider.</param>
        /// <param name="intervalMin">The interval minimum.</param>
        /// <param name="intervalMax">The interval maximum.</param>
        /// <param name="startDelayMin">The start delay minimum.</param>
        /// <param name="startDelayMax">The start delay maximum.</param>
        /// <returns></returns>
        public int AddClient(Guid aiId, IContextProvider contextProvider, float intervalMin, float intervalMax, float startDelayMin, float startDelayMax)
        {
            var ai = AIManager.GetAI(aiId);
            if (ai == null)
            {
                Debug.LogWarning(this.gameObject.name + ": Unable to load AI, no AI with the specified ID exists.");
                return -1;
            }

            var aiConfig = new UtilityAIConfig()
            {
                aiId = aiId.ToString(),
                intervalMin = intervalMin,
                intervalMax = intervalMax,
                startDelayMin = startDelayMin,
                startDelayMax = startDelayMax,
                isActive = true
            };

            if (_usedClients == this.aiConfigs.Length)
            {
                Resize(ref this.aiConfigs, Mathf.Max(2, this.aiConfigs.Length * 2));
            }

            aiConfigs[_usedClients] = aiConfig;

            var client = new LoadBalancedUtilityAIClient(aiId, contextProvider, intervalMin, intervalMax, startDelayMin, startDelayMax);
            return AddClient(client);
        }

        private int AddClient(IUtilityAIClient client)
        {
            var c = this.clients;
            if (_usedClients == c.Length)
            {
                Resize(ref _clients, Mathf.Max(2, c.Length * 2));
            }

            _clients[_usedClients++] = client;

            if (OnNewAI != null)
            {
                OnNewAI(client);
            }

            return _usedClients - 1;
        }

        /// <summary>
        /// Removes the AI client using the AI ID specified.
        /// </summary>
        /// <param name="aiId">The ai identifier.</param>
        /// <returns></returns>
        public bool RemoveClient(Guid aiId)
        {
            var c = this.clients;
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] != null && c[i].ai.id == aiId)
                {
                    RemoveClientAt(i);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Removes the AI client.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <returns></returns>
        public bool RemoveClient(IUtilityAIClient client)
        {
            var c = this.clients;
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] != null && c[i].Equals(client))
                {
                    RemoveClientAt(i);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Removes the client at the given index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">index</exception>
        public void RemoveClientAt(int index)
        {
            var c = this.clients;
            if (index < 0 || index >= c.Length)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            int idxLast = _usedClients - 1;
            if (index < idxLast)
            {
                c[index] = c[idxLast];
            }

            c[idxLast] = null;
            _usedClients--;
        }

        /// <summary>
        /// Pauses all AI clients.
        /// </summary>
        public void Pause()
        {
            var c = this.clients;
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] != null)
                {
                    c[i].Pause();
                }
            }
        }

        /// <summary>
        /// Resumes all previously paused AI clients.
        /// </summary>
        public void Resume()
        {
            var c = this.clients;
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] != null)
                {
                    c[i].Resume();
                }
            }
        }

        internal void ToggleActive(int idx, bool active)
        {
            if (this.aiConfigs[idx].isActive == active)
            {
                return;
            }

            this.aiConfigs[idx].isActive = active;

            if (Application.isPlaying)
            {
                if (active)
                {
                    this.clients[idx].Start();
                }
                else
                {
                    this.clients[idx].Stop();
                }
            }
        }

        /// <summary>
        /// Called on Start and OnEnable, but only one of the two, i.e. at startup it is only called once.
        /// </summary>
        protected override void OnStartAndEnable()
        {
            var c = this.clients;
            for (int i = 0; i < c.Length; i++)
            {
				var config = this.aiConfigs[i];
				if (config == null)
				{
					continue;
				}
				
                if (c[i] != null && config.isActive)
                {
                    c[i].Start();
                }
            }
        }

        private void OnDisable()
        {
            var c = this.clients;
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] != null)
                {
                    c[i].Stop();
                }
            }
        }

        private void Resize<T>(ref T[] array, int newCapacity)
        {
            var tmp = new T[newCapacity];
            Array.Copy(array, 0, tmp, 0, array.Length);
            array = tmp;
        }
    }
}