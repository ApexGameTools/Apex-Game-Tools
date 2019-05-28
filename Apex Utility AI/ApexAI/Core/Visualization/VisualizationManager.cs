namespace Apex.AI.Visualization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Components;
    using UnityEngine;
    using Utilities;

    /// <summary>
    /// Manager that handles AI Visualization.
    /// </summary>
    public static class VisualizationManager
    {
        private static bool _visualizing;
        private static Dictionary<Type, ICustomVisualizer> _visualizerLookup;
        private static List<IContextProvider> _visualizedContextProviders;

        /// <summary>
        /// Gets a value indicating whether AI visualization is currently active.
        /// </summary>
        /// <value>
        /// <c>true</c> if visualization is active; otherwise, <c>false</c>.
        /// </value>
        internal static bool isVisualizing
        {
            get { return _visualizing; }
        }

        internal static IList<IContextProvider> visualizedContextProviders
        {
            get
            {
                if (_visualizedContextProviders == null)
                {
                    _visualizedContextProviders = new List<IContextProvider>();
                }

                return _visualizedContextProviders;
            }
        }

        /// <summary>
        /// Registers a custom visualizer.
        /// </summary>
        /// <typeparam name="TFor">The type visualized by the custom visualizer.</typeparam>
        /// <param name="visualizer">The visualizer.</param>
        /// <param name="registerDerivedTypes">Whether to register the visualizer for all types derived from <typeparamref name="TFor"/>.</param>
        public static void RegisterVisualizer<TFor>(ICustomVisualizer visualizer, bool registerDerivedTypes = false)
        {
            RegisterVisualizer(typeof(TFor), visualizer, registerDerivedTypes);
        }

        /// <summary>
        /// Registers a custom visualizer.
        /// </summary>
        /// <param name="forType">The type visualized by the custom visualizer.</param>
        /// <param name="visualizer">The visualizer.</param>
        /// <param name="registerDerivedTypes">Whether to register the visualizer for all types derived from <paramref name="forType"/></param>
        public static void RegisterVisualizer(Type forType, ICustomVisualizer visualizer, bool registerDerivedTypes = false)
        {
            Ensure.ArgumentNotNull(visualizer, "visualizer");

            if (_visualizerLookup == null)
            {
                _visualizerLookup = new Dictionary<Type, ICustomVisualizer>();
            }

            if (forType.IsAbstract || registerDerivedTypes)
            {
                var types = GetDerived(forType);
                foreach (var t in types)
                {
                    if (_visualizerLookup.ContainsKey(t))
                    {
                        Debug.LogWarning(string.Format("A visualizer for type {0} has already been registered, skipping {1}.", t.Name, visualizer.GetType().Name));
                    }
                    else
                    {
                        _visualizerLookup.Add(t, visualizer);
                    }
                }
            }
            else
            {
                if (_visualizerLookup.ContainsKey(forType))
                {
                    Debug.LogWarning(string.Format("A visualizer for type {0} has already been registered, skipping {1}.", forType.Name, visualizer.GetType().Name));
                    return;
                }

                _visualizerLookup.Add(forType, visualizer);
            }
        }

        /// <summary>
        /// Unregisters a visualizer.
        /// </summary>
        /// <typeparam name="TFor">The type for which to unregister the visualizer.</typeparam>
        /// <param name="registeredDerivedTypes">Whether the visualizer was registered for all types derived from <typeparamref name="TFor"/></param>
        public static void UnregisterVisualizer<TFor>(bool registeredDerivedTypes = false)
        {
            UnregisterVisualizer(typeof(TFor), registeredDerivedTypes);
        }

        /// <summary>
        /// Unregisters a visualizer.
        /// </summary>
        /// <param name="forType">The type for which to unregister the visualizer.</param>
        /// <param name="registeredDerivedTypes">Whether the visualizer was registered for all types derived from <paramref name="forType"/></param>
        public static void UnregisterVisualizer(Type forType, bool registeredDerivedTypes = false)
        {
            if (_visualizerLookup == null)
            {
                return;
            }

            if (forType.IsAbstract || registeredDerivedTypes)
            {
                var types = GetDerived(forType);
                foreach (var t in types)
                {
                    _visualizerLookup.Remove(t);
                }
            }
            else
            {
                _visualizerLookup.Remove(forType);
            }
        }

        internal static bool BeginVisualization()
        {
            if (_visualizing)
            {
                return false;
            }

            _visualizing = true;

            foreach (var client in AIManager.allClients)
            {
                if (!(client.ai is UtilityAIVisualizer))
                {
                    client.ai = new UtilityAIVisualizer(client.ai);
                }
            }

            return true;
        }

        internal static void UpdateSelectedGameObjects(GameObject[] selected)
        {
            if (!_visualizing)
            {
                return;
            }

            //Lazy loaded so use property
            var vcp = visualizedContextProviders;
            vcp.Clear();
            if (selected == null)
            {
                return;
            }

            for (int i = 0; i < selected.Length; i++)
            {
                var contextProvider = selected[i].GetComponent(typeof(IContextProvider)) as IContextProvider;
                if (contextProvider != null)
                {
                    vcp.Add(contextProvider);
                }
            }
        }

        internal static bool TryGetVisualizerFor(Type t, out ICustomVisualizer visualizer)
        {
            if (_visualizerLookup == null)
            {
                visualizer = null;
                return false;
            }

            return _visualizerLookup.TryGetValue(t, out visualizer);
        }

        private static IEnumerable<Type> GetDerived(Type forType)
        {
            return from t in ApexReflection.GetRelevantTypes()
                   where t.IsClass && !t.IsAbstract && forType.IsAssignableFrom(t)
                   select t;
        }
    }
}
