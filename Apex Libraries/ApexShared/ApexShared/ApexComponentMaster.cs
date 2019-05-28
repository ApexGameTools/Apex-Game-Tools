/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex
{
    using System.Collections.Generic;
    using System.Linq;
    using Apex.DataStructures;
    using UnityEngine;

    /// <summary>
    /// Consolidates Apex Components
    /// </summary>
    [AddComponentMenu("Apex/Common/Apex Component Master", 1000)]
    public class ApexComponentMaster : MonoBehaviour
    {
        private Dictionary<int, ComponentInfo> _components = new Dictionary<int, ComponentInfo>();
        private Dictionary<string, ComponentCategory> _categories = new Dictionary<string, ComponentCategory>();

        [SerializeField, HideInInspector]
        private bool _firstTime = true;

        [SerializeField, HideInInspector]
        private int _hiddenComponents;

        /// <summary>
        /// Gets the component categories.
        /// </summary>
        /// <value>
        /// The component categories.
        /// </value>
        public IEnumerable<ComponentCategory> componentCategories
        {
            get
            {
                var sortedCategories = _categories.Keys.OrderBy(s => s);
                foreach (var catName in sortedCategories)
                {
                    yield return _categories[catName];
                }
            }
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <returns></returns>
        public bool Init(IEnumerable<ComponentCandidate> candidates)
        {
            Cleanup();

            bool updated = false;
            int idx = 0;
            foreach (var cc in candidates)
            {
                var id = cc.component.GetInstanceID();
                ComponentInfo cinfo;
                bool alreadyRegistered = _components.TryGetValue(id, out cinfo);

                //Since hideflags are reset when applying prefab changes, we reapply them here
                if ((cc.component.hideFlags & HideFlags.HideInInspector) == 0 && (_hiddenComponents & (1 << idx)) > 0)
                {
                    cc.component.hideFlags |= HideFlags.HideInInspector;
                    updated = true;
                }

                //If already registered we still need to ensure hideflags are set correctly, again due to prefab stuff
                if (alreadyRegistered)
                {
                    idx++;
                    cinfo.isVisible = (cc.component.hideFlags & HideFlags.HideInInspector) == 0;
                    continue;
                }

                cinfo = new ComponentInfo
                {
                    component = cc.component,
                    id = id,
                    idx = idx++,
                    name = cc.component.GetType().Name.Replace("Component", string.Empty).ExpandFromPascal(),
                    isVisible = (cc.component.hideFlags & HideFlags.HideInInspector) == 0
                };

                _components.Add(id, cinfo);

                ComponentCategory cat;
                if (!_categories.TryGetValue(cc.categoryName, out cat))
                {
                    cat = new ComponentCategory { name = cc.categoryName };
                    _categories.Add(cc.categoryName, cat);
                }

                cat.Add(cinfo);
                cinfo.category = cat;
            }

            var comparer = new FunctionComparer<ComponentInfo>((a, b) => a.name.CompareTo(b.name));
            foreach (var cat in _categories.Values)
            {
                cat.Sort(comparer);
            }

            if (_firstTime)
            {
                ToggleAll();
                _firstTime = false;
                return true;
            }

            return updated;
        }

        /// <summary>
        /// Toggles the specified component.
        /// </summary>
        /// <param name="cinfo">The component info.</param>
        public void Toggle(ComponentInfo cinfo)
        {
            cinfo.isVisible = !cinfo.isVisible;

            var current = cinfo.component.hideFlags;
            if (cinfo.isVisible)
            {
                RemoveHidden(cinfo);
                cinfo.component.hideFlags = (current & ~HideFlags.HideInInspector);
            }
            else
            {
                AddHidden(cinfo);
                cinfo.component.hideFlags = (current | HideFlags.HideInInspector);
            }
        }

        /// <summary>
        /// Toggles the specified component by name.
        /// </summary>
        /// <param name="componentName">Name of the component.</param>
        /// <param name="visible">toggle visible or not.</param>
        public void Toggle(string componentName, bool visible)
        {
            _components
                .Values.Where(c => c.name == componentName)
                .Apply(c =>
                    {
                        c.isVisible = visible;
                        var current = c.component.hideFlags;
                        if (visible)
                        {
                            RemoveHidden(c);
                            c.component.hideFlags = (current & ~HideFlags.HideInInspector);
                        }
                        else
                        {
                            AddHidden(c);
                            c.component.hideFlags = (current | HideFlags.HideInInspector);
                        }
                    });
        }

        /// <summary>
        /// Toggles all components
        /// </summary>
        public void ToggleAll()
        {
            bool visible = !_components.Values.Any(c => c.isVisible);
            if (visible)
            {
                _hiddenComponents = 0;
            }
            else
            {
                _hiddenComponents = int.MaxValue >> (31 - _components.Values.Count);
            }

            foreach (var c in _components.Values)
            {
                c.isVisible = visible;
                var current = c.component.hideFlags;
                c.component.hideFlags = visible ? (current & ~HideFlags.HideInInspector) : (current | HideFlags.HideInInspector);
            }
        }

        /// <summary>
        /// Cleanups this instance.
        /// </summary>
        public void Cleanup()
        {
            ComponentInfo toRemove = null;
            foreach (var c in _components.Values)
            {
                if (c.component.Equals(null))
                {
                    toRemove = c;
                }
            }

            if (toRemove != null)
            {
                RemoveHidden(toRemove);
                _components.Remove(toRemove.id);
                toRemove.category.Remove(toRemove);

                if (toRemove.category.count == 0)
                {
                    _categories.Remove(toRemove.category.name);
                }
            }
        }

        private void AddHidden(ComponentInfo c)
        {
            if (c.idx > 29)
            {
                Debug.LogWarning("Apex Component Master cannot manage more than 30 components.");
                return;
            }

            _hiddenComponents |= (1 << c.idx);
        }

        private void RemoveHidden(ComponentInfo c)
        {
            if (c.idx > 29)
            {
                Debug.LogWarning("Apex Component Master cannot manage more than 30 components.");
                return;
            }

            _hiddenComponents &= ~(1 << c.idx);
        }

        /// <summary>
        /// Category wrapper
        /// </summary>
        public class ComponentCategory : DynamicArray<ComponentInfo>
        {
            public bool isOpen;
            public string name;

            public ComponentCategory()
                : base(5)
            {
            }
        }

        public class ComponentInfo
        {
            public MonoBehaviour component;

            public ComponentCategory category;

            public string name;

            public int id;

            public int idx;

            public bool isVisible;
        }

        public class ComponentCandidate
        {
            public MonoBehaviour component;

            public string categoryName;
        }
    }
}
