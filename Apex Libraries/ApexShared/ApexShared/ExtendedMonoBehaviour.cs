/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex
{
    using UnityEngine;

    /// <summary>
    /// MonoBehaviour extension that couples Start and OnEnable to ensure that certain initialization logic is performed in both instances but only once.
    /// </summary>
    public abstract class ExtendedMonoBehaviour : MonoBehaviour
    {
        private bool _hasStarted;

        /// <summary>
        /// Called on Start
        /// </summary>
        protected virtual void Start()
        {
            _hasStarted = true;
            OnStartAndEnable();
        }

        /// <summary>
        /// Called when enabled.
        /// </summary>
        protected virtual void OnEnable()
        {
            if (_hasStarted)
            {
                OnStartAndEnable();
            }
        }

        /// <summary>
        /// Called on Start and OnEnable, but only one of the two, i.e. at startup it is only called once.
        /// </summary>
        protected virtual void OnStartAndEnable()
        {
        }
    }
}
