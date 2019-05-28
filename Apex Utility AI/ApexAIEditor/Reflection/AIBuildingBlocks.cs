/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Apex.Utilities;

    public sealed class AIBuildingBlocks
    {
        private static readonly object _instanceLock = new object();
        private static AIBuildingBlocks _instance;

        private List<NamedType> _selectors = new List<NamedType>();
        private List<NamedType> _qualifiers = new List<NamedType>();
        private List<NamedType> _actions = new List<NamedType>();

        private Dictionary<Type, List<NamedType>> _customItems = new Dictionary<Type, List<NamedType>>();

        private AIBuildingBlocks()
        {
        }

        public static AIBuildingBlocks Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_instanceLock)
                    {
                        if (_instance == null)
                        {
                            _instance = new AIBuildingBlocks();
                            _instance.Init();
                        }
                    }
                }

                return _instance;
            }
        }

        public IEnumerable<NamedType> selectors
        {
            get { return _selectors; }
        }

        public IEnumerable<NamedType> qualifiers
        {
            get { return _qualifiers; }
        }

        public IEnumerable<NamedType> actions
        {
            get { return _actions; }
        }

        public IEnumerable<NamedType> GetForType(Type t)
        {
            List<NamedType> list;
            if (_customItems.TryGetValue(t, out list))
            {
                return list;
            }

            var derivatives = ReflectDerivatives(t);
            list = Wrap(derivatives);

            _customItems[t] = list;
            return list;
        }

        private static List<NamedType> Wrap(IEnumerable<Type> types)
        {
            var res = from t in types
                      let at = new NamedType(t)
                      orderby at.friendlyName
                      select at;

            return res.ToList();
        }

        private static IEnumerable<Type> GetConstructableTypes()
        {
            return from t in ApexReflection.GetRelevantTypes()
                   where !t.IsAbstract && !t.IsDefined<HiddenAttribute>(false) && t.GetConstructor(Type.EmptyTypes) != null
                   select t;
        }

        private static IEnumerable<Type> ReflectDerivatives(Type forType)
        {
            var relevantTypes = GetConstructableTypes();

            return ReflectDerivatives(forType, relevantTypes);
        }

        private static IEnumerable<Type> ReflectDerivatives(Type forType, IEnumerable<Type> relevantTypes)
        {
            return from t in relevantTypes
                   where (t.IsSubclassOf(forType) || forType.IsAssignableFrom(t) || t.Equals(forType))
                   select t;
        }

        private void Init()
        {
            var relevantTypes = GetConstructableTypes().ToArray();

            var selectors = ReflectDerivatives(typeof(Selector), relevantTypes);

            var qualifiers = ReflectDerivatives(typeof(IQualifier), relevantTypes);

            var actions = ReflectDerivatives(typeof(IAction), relevantTypes);

            _selectors = Wrap(selectors);
            _qualifiers = Wrap(qualifiers);
            _actions = Wrap(actions);

            //Register qualifiers and actions for composites, no need to have them duplicated.
            _customItems[typeof(Selector)] = _selectors;
            _customItems[typeof(IQualifier)] = _qualifiers;
            _customItems[typeof(IAction)] = _actions;
        }

        public class NamedType : IHaveFriendlyName
        {
            internal NamedType(Type t)
            {
                this.type = t;

                var nameAttrib = t.GetAttribute<FriendlyNameAttribute>(true);

                this.friendlyName = (nameAttrib != null) ? nameAttrib.name : t.PrettyName();
                this.description = (nameAttrib != null) ? nameAttrib.description : null;
            }

            public string friendlyName
            {
                get;
                internal set;
            }

            public string description
            {
                get;
                internal set;
            }

            public Type type
            {
                get;
                internal set;
            }
        }
    }
}