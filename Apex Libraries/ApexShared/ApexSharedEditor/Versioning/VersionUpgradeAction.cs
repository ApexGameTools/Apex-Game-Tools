namespace Apex.Editor.Versioning
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

    public abstract class VersionUpgradeAction
    {
        protected VersionUpgradeAction()
        {
        }

        protected delegate void Initializer<T>(GameObject go, T item, bool added);

        public int priority
        {
            get;
            protected set;
        }

        public string name
        {
            get;
            protected set;
        }

        public bool isOptional
        {
            get;
            protected set;
        }

        public abstract bool Upgrade();

        protected static bool Nuke<T>() where T : Component
        {
            var res = Resources.FindObjectsOfTypeAll<T>();
            foreach (var c in res)
            {
                Component.DestroyImmediate(c, true);
            }

            return res.Length > 0;
        }

        protected static bool Replace<T, TNew>(Action<T, TNew> configure = null)
            where T : Component
            where TNew : Component
        {
            var res = GetAllNonPrefabInstances<T>();
            bool changed = res.Any(); //important to do that here and not after the iteration, since they are destroyed.
            foreach (var c in res)
            {
                var go = c.gameObject;
                var cnew = go.AddComponent<TNew>();
                cnew.hideFlags = c.hideFlags;

                if (configure != null)
                {
                    configure(c, cnew);
                }

                Component.DestroyImmediate(c, true);
            }

            return changed;
        }

        protected static bool AddItem<T>(GameObject target, Initializer<T> init = null) where T : Component
        {
            T item;
            var added = target.AddIfMissing<T>(false, out item);

            if (init != null)
            {
                init(target, item, added);
            }

            return added;
        }

        protected static IEnumerable<T> GetAllNonPrefabInstances<T>() where T : UnityEngine.Object
        {
            //Keep this an IEnumerable to allow for Destroy being called during iteration, as such instance will then be skipped
            return from item in Resources.FindObjectsOfTypeAll<T>()
                   where item != null && !item.Equals(null) && PrefabUtility.GetPrefabType(item) != PrefabType.PrefabInstance
                   select item;
        }

        protected static bool SetPrivate(object target, string name, object value)
        {
            var t = target.GetType();

            var f = t.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            if (f != null && !object.Equals(f.GetValue(target), value))
            {
                f.SetValue(target, value);
                return true;
            }

            return false;
        }
    }
}
