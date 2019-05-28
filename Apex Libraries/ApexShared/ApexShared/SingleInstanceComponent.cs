/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex
{
    using UnityEngine;

    /// <summary>
    /// A component of which only one instance will be allowed to exist in the game world. Its a singleton but not referable as such.
    /// </summary>
    /// <typeparam name="T">The type of the derived behaviour</typeparam>
    public abstract class SingleInstanceComponent<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static int _instanceMark;

        private void Awake()
        {
            if (_instanceMark > 0)
            {
                Debug.LogWarning(string.Format("Removing superfluous {0} from scene.", typeof(T).Name));
                Destroy(this);

                return;
            }

            _instanceMark = 1;

            OnAwake();
        }

        /// <summary>
        /// Called when destroyed.
        /// </summary>
        protected virtual void OnDestroy()
        {
            _instanceMark = 0;
        }

        /// <summary>
        /// Called on awake.
        /// </summary>
        protected virtual void OnAwake()
        {
        }
    }
}
