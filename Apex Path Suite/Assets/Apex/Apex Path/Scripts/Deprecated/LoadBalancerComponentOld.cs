/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.LoadBalancing
{
    using UnityEngine;

    [AddComponentMenu("")]
    public class LoadBalancerComponentOld : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        public LoadBalancerConfig[] _configurations;

        [SerializeField]
        public int _mashallerMaxMillisecondPerFrame = 4;

        private void Awake()
        {
            Debug.LogWarning("This scene contains deprecated components, please run Tools -> Apex -> Update Scene.");
        }
    }
}
