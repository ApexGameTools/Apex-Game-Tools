/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.LoadBalancing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEngine;

    /// <summary>
    /// Component for configuring the various <see cref="LoadBalancedQueue"/>s exposed by the <see cref="LoadBalancer"/> and derivatives.
    /// </summary>
    [AddComponentMenu("Apex/Game World/Load Balancer", 1036)]
    [ApexComponent("Game World")]
    public partial class LoadBalancerComponent : SingleInstanceComponent<LoadBalancerComponent>
    {
        [SerializeField, HideInInspector]
        private LoadBalancerConfig[] _configurations;

        [SerializeField, HideInInspector]
        private int _mashallerMaxMillisecondPerFrame = 4;

        private LoadBalancedQueue[] _loadBalancers;
        private Marshaller _marshaller;

        /// <summary>
        /// Gets configurations for all resolved load balancers
        /// </summary>
        public LoadBalancerConfig[] configurations
        {
            get
            {
                //The test is against the load balancers since we are interested in knowing if the resolution has run or not. The configurations member will not be null since it is serialized.
                if (_loadBalancers == null)
                {
                    ResolveLoadBalancers();
                }

                return _configurations;
            }
        }

        /// <summary>
        /// Gets all resolved load balancers.
        /// </summary>
        public IEnumerable<LoadBalancedQueue> loadBalancers
        {
            get
            {
                if (_loadBalancers == null)
                {
                    ResolveLoadBalancers();
                }

                return _loadBalancers;
            }
        }

        /// <summary>
        /// Called on awake.
        /// </summary>
        protected override void OnAwake()
        {
            ResolveLoadBalancers();

            _marshaller = new Marshaller(_mashallerMaxMillisecondPerFrame);
            LoadBalancer.marshaller = _marshaller;
        }

        private void Update()
        {
            for (int i = 0; i < _loadBalancers.Length; i++)
            {
                _loadBalancers[i].Update();
            }

            _marshaller.ProcessPending();
        }

        private void ResolveLoadBalancers()
        {
            var resolveBalancers = new List<LoadBalancedQueue>();
            var configSet = new Dictionary<string, LoadBalancerConfig>(StringComparer.Ordinal);

            if (_configurations != null)
            {
                foreach (var cfg in _configurations)
                {
                    configSet.Add(cfg.targetLoadBalancer, cfg);
                }
            }

            var lbType = typeof(LoadBalancer);
            var qType = typeof(LoadBalancedQueue);
            var qTypeAlt = typeof(ILoadBalancer);

#if NETFX_CORE
            var lbTypeInf = lbType.GetTypeInfo();
            var asm = lbTypeInf.Assembly;

            var sources = (from t in asm.DefinedTypes
                           where t == lbTypeInf || t.IsSubclassOf(lbType)
                           select t).ToArray();

            //Process properties
            var props = from t in sources
                        from p in t.DeclaredProperties
                        where (p.PropertyType == qType || p.PropertyType == qTypeAlt) && p.CanRead && p.GetMethod.IsStatic
                        select p;
#elif UNITY_WP8
            var asm = lbType.Assembly;

            var sources = (from t in asm.GetTypes()
                           where t == lbType || t.IsSubclassOf(lbType)
                           select t).ToArray();

            //Process properties
            var props = from t in sources
                        from p in t.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly)
                        where (p.PropertyType == qType || p.PropertyType == qTypeAlt) && p.CanRead
                        select p;
#else
            var sources = (from asm in AppDomain.CurrentDomain.GetAssemblies()
                           from t in asm.GetTypes()
                           where t == lbType || t.IsSubclassOf(lbType)
                           select t).ToArray();

            //Process properties
            var props = from t in sources
                        from p in t.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly)
                        where (p.PropertyType == qType || p.PropertyType == qTypeAlt) && p.CanRead
                        select p;
#endif
            LoadBalancerConfig config;
            foreach (var p in props)
            {
                var lbName = p.Name;

                var balancer = p.GetValue(null, null) as LoadBalancedQueue;
                if (balancer == null && p.CanWrite)
                {
                    balancer = new LoadBalancedQueue(4);
                    p.SetValue(null, balancer, null);
                }

                if (balancer != null)
                {
                    if (!configSet.TryGetValue(lbName, out config))
                    {
                        config = LoadBalancerConfig.From(lbName, balancer);
                        configSet.Add(lbName, config);
                    }
                    else
                    {
                        config.ApplyTo(balancer);
                    }

                    resolveBalancers.Add(balancer);
                }
            }

            //Process fields
#if NETFX_CORE
            var fields = from t in sources
                         from f in t.DeclaredFields
                         where (f.FieldType == qType || f.FieldType == qTypeAlt) && f.IsStatic
                         select f;
#else
            var fields = from t in sources
                         from f in t.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly)
                         where (f.FieldType == qType || f.FieldType == qTypeAlt)
                         select f;
#endif
            foreach (var f in fields)
            {
                var lbName = f.Name;

                var balancer = f.GetValue(null) as LoadBalancedQueue;
                if (balancer == null && !f.IsInitOnly)
                {
                    balancer = new LoadBalancedQueue(4);
                    f.SetValue(null, balancer);
                }

                if (balancer != null)
                {
                    if (!configSet.TryGetValue(lbName, out config))
                    {
                        config = LoadBalancerConfig.From(lbName, balancer);
                        configSet.Add(lbName, config);
                    }
                    else
                    {
                        config.ApplyTo(balancer);
                    }

                    resolveBalancers.Add(balancer);
                }
            }

            _configurations = configSet.Values.Where(c => c.associatedLoadBalancer != null).OrderBy(c => c.targetLoadBalancer).ToArray();
            _loadBalancers = resolveBalancers.ToArray();
        }
    }
}
