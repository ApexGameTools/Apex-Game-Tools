/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex
{
    using UnityEngine;

    /// <summary>
    /// Consolidates Apex Components
    /// </summary>
    [AddComponentMenu("")]
    public class ApexComponentMasterOld : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        public bool _firstTime = true;
    }
}
