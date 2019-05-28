/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Components
{
    using System;

    /// <summary>
    /// Configuration settings for AIs 
    /// </summary>
    [Serializable]
    internal class UtilityAIConfig
    {
        public string aiId;
        public float intervalMin;
        public float intervalMax;
        public float startDelayMin;
        public float startDelayMax;
        public bool isActive;
    }
}
