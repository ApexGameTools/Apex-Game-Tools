/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Utilities
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    /// <summary>
    /// Utility class to help with certain Component related tasks
    /// </summary>
    public static class ComponentHelper
    {
        /// <summary>
        /// Finds the first component of the specified type in the scene.
        /// </summary>
        /// <typeparam name="T">The type of component to look for</typeparam>
        /// <returns>The component or <c>null</c> if not found</returns>
        public static T FindFirstComponentInScene<T>() where T : Component
        {
            var res = UnityEngine.Object.FindObjectsOfType(typeof(T));

            if (res != null && res.Length > 0)
            {
                return res[0] as T;
            }

            return null;
        }

        /// <summary>
        /// Finds all components of the specified type in the scene.
        /// </summary>
        /// <typeparam name="T">The type of component to look for</typeparam>
        /// <returns>The component or <c>null</c> if not found</returns>
        public static IEnumerable<T> FindAllComponentsInScene<T>() where T : Component
        {
            return UnityEngine.Object.FindObjectsOfType(typeof(T)).Cast<T>();
        }
    }
}
