/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Utilities;

    /// <summary>
    /// Various extension to Unity types.
    /// </summary>
    public static class SharedUnityExtensions
    {
        private static readonly Plane _xzPlane = new Plane(Vector3.up, Vector3.zero);

        /// <summary>
        /// Gets the collider at position.
        /// </summary>
        /// <param name="cam">The camera.</param>
        /// <param name="screenPos">The screen position.</param>
        /// <param name="layerMask">The layer mask.</param>
        /// <param name="maxDistance">The maximum distance.</param>
        /// <returns>The first collider found in the game world at the specified screen position.</returns>
        public static Collider GetColliderAtPosition(this Camera cam, Vector3 screenPos, LayerMask layerMask, float maxDistance = 1000.0f)
        {
            RaycastHit hit;
            var ray = cam.ScreenPointToRay(screenPos);

            if (Physics.Raycast(ray, out hit, maxDistance, layerMask))
            {
                return hit.collider;
            }

            return null;
        }

        /// <summary>
        /// Casts a ray from the camera to the specified position.
        /// </summary>
        /// <param name="cam">The camera.</param>
        /// <param name="screenPos">The screen position.</param>
        /// <param name="layerMask">The layer mask.</param>
        /// <param name="maxDistance">The maximum distance.</param>
        /// <param name="hit">The hit details.</param>
        /// <returns><c>true</c> if the ray hit something, otherwise <c>false</c></returns>
        public static bool ScreenToLayerHit(this Camera cam, Vector3 screenPos, LayerMask layerMask, float maxDistance, out RaycastHit hit)
        {
            var ray = cam.ScreenPointToRay(screenPos);
            return Physics.Raycast(ray, out hit, maxDistance, layerMask);
        }

        /// <summary>
        /// Casts a ray from the camera to the xz plane through the specified screen point and returns the point the ray intersects the xz plane in world coordinates.
        /// </summary>
        /// <param name="cam">The camera.</param>
        /// <param name="screenPos">The screen position.</param>
        /// <returns>The intersection point on the xz plane in world coordinates</returns>
        public static Vector3 ScreenToGroundPoint(this Camera cam, Vector3 screenPos)
        {
            var ray = cam.ScreenPointToRay(screenPos);

            float d;
            if (_xzPlane.Raycast(ray, out d))
            {
                return ray.GetPoint(d);
            }

            return Vector3.zero;
        }

        /// <summary>
        /// Casts a ray from the camera to the xz plane through the specified screen point and returns the point the ray intersects the xz plane in world coordinates.
        /// </summary>
        /// <param name="cam">The camera.</param>
        /// <param name="screenPos">The screen position.</param>
        /// <param name="groundHeight">Height (y-coordinate) that the ground level is at.</param>
        /// <returns>The intersection point on the xz plane in world coordinates</returns>
        public static Vector3 ScreenToGroundPoint(this Camera cam, Vector3 screenPos, float groundHeight)
        {
            var ray = cam.ScreenPointToRay(screenPos);
            var xzElevatedPlane = new Plane(Vector3.up, new Vector3(0f, groundHeight, 0f));

            float d;
            if (xzElevatedPlane.Raycast(ray, out d))
            {
                return ray.GetPoint(d);
            }

            return Vector3.zero;
        }

        /// <summary>
        /// Casts a ray from the camera to the xz plane through the specified viewport point and returns the point the ray intersects the xz plane in world coordinates.
        /// </summary>
        /// <param name="cam">The camera.</param>
        /// <param name="screenPos">The viewport position.</param>
        /// <returns>The intersection point on the xz plane in world coordinates</returns>
        public static Vector3 ViewportToGroundPoint(this Camera cam, Vector3 screenPos)
        {
            var ray = cam.ViewportPointToRay(screenPos);

            float d;
            if (_xzPlane.Raycast(ray, out d))
            {
                return ray.GetPoint(d);
            }

            return Vector3.zero;
        }


        /// <summary>
        /// Checks if one vector is approximately equal to another
        /// </summary>
        /// <param name="me">Me.</param>
        /// <param name="other">The other.</param>
        /// <param name="allowedDifference">The allowed difference.</param>
        /// <returns><c>true</c> if the are approximately equal, otherwise <c>false</c></returns>
        public static bool Approximately(this Vector3 me, Vector3 other, float allowedDifference)
        {
            var dx = me.x - other.x;
            if (dx < -allowedDifference || dx > allowedDifference)
            {
                return false;
            }

            var dy = me.y - other.y;
            if (dy < -allowedDifference || dy > allowedDifference)
            {
                return false;
            }

            var dz = me.z - other.z;

            return (dz >= -allowedDifference) && (dz <= allowedDifference);
        }

        /// <summary>
        /// Get the direction between two point in the xz plane only
        /// </summary>
        /// <param name="from">The from position.</param>
        /// <param name="to">The to position.</param>
        /// <returns>The direction vector between the two points.</returns>
        public static Vector3 DirToXZ(this Vector3 from, Vector3 to)
        {
            return new Vector3(to.x - from.x, 0f, to.z - from.z);
        }

        /// <summary>
        /// Discards the y-component of the vector
        /// </summary>
        /// <param name="v">The vector.</param>
        /// <returns>The vector with y set to 0</returns>
        public static Vector3 OnlyXZ(this Vector3 v)
        {
            v.y = 0f;
            return v;
        }

        /// <summary>
        /// Gets the first MonoBehavior on the component's game object that is of type T. This is different from GetComponent in that the type can be an interface or class that is not itself a component.
        /// It is however a relatively slow operation, and should not be used in actions that happen frequently, e.g. Update.
        /// </summary>
        /// <typeparam name="T">The type of behavior to look for</typeparam>
        /// <param name="c">The component whose siblings will be inspected if they are of type T</param>
        /// <param name="searchParent">if set to <c>true</c> the parent transform will also be inspected if no match is found on the current component's transform.</param>
        /// <param name="required">if set to <c>true</c> and the requested component is not found, an exception will be thrown.</param>
        /// <returns>
        /// The T behavior sibling of the component or null if not found.
        /// </returns>
        public static T As<T>(this Component c, bool searchParent = false, bool required = false) where T : class
        {
            if (c.Equals(null))
            {
                return null;
            }

            return As<T>(c.gameObject, searchParent, required);
        }

        /// <summary>
        /// Gets the first MonoBehavior on the component's game object that is of type T. This is different from GetComponent in that the type can be an interface or class that is not itself a component.
        /// It is however a relatively slow operation, and should not be used in actions that happen frequently, e.g. Update.
        /// </summary>
        /// <typeparam name="T">The type of behavior to look for</typeparam>
        /// <param name="c">The component whose siblings will be inspected if they are of type T</param>
        /// <param name="searchParent">if set to <c>true</c> the parent transform will also be inspected if no match is found on the current component's transform.</param>
        /// <param name="required">if set to <c>true</c> and the requested component is not found, an exception will be thrown.</param>
        /// <returns>
        /// The T behavior sibling of the component or null if not found.
        /// </returns>
        public static T As<T>(this IGameObjectComponent c, bool searchParent = false, bool required = false) where T : class
        {
            if (c.Equals(null))
            {
                return null;
            }

            return As<T>(c.gameObject, searchParent, required);
        }

        /// <summary>
        /// Gets the first MonoBehavior on the game object that is of type T. This is different from GetComponent in that the type can be an interface or class that is not itself a component.
        /// It is however a relatively slow operation, and should not be used in actions that happen frequently, e.g. Update.
        /// </summary>
        /// <typeparam name="T">The type of behavior to look for</typeparam>
        /// <param name="go">The game object whose components will be inspected if they are of type T</param>
        /// <param name="searchParent">if set to <c>true</c> the parent transform will also be inspected if no match is found on the current game object.</param>
        /// <param name="required">if set to <c>true</c> and the requested component is not found, an exception will be thrown.</param>
        /// <returns>
        /// The T behavior or null if not found.
        /// </returns>
        public static T As<T>(this GameObject go, bool searchParent = false, bool required = false) where T : class
        {
            if (go.Equals(null))
            {
                return null;
            }

            var c = go.GetComponent(typeof(T)) as T;

            if (c == null && searchParent && go.transform.parent != null)
            {
                return As<T>(go.transform.parent.gameObject, false, required);
            }

            if (c == null && required)
            {
                throw new MissingComponentException(string.Format("Game object {0} does not have a component of type {1}.", go.name, typeof(T).Name));
            }

            return c;
        }

        /// <summary>
        /// Warns if multiple instances of the component exists on its game object.
        /// </summary>
        /// <param name="component">The component.</param>
        public static void WarnIfMultipleInstances(this MonoBehaviour component)
        {
            var t = component.GetType();

            if (component.GetComponents(t).Length > 1)
            {
                Debug.LogWarning(string.Format("GameObject '{0}' defines multiple instances of '{1}' which is not recommended.", component.gameObject.name, t.Name));
            }
        }

        /// <summary>
        /// Warns if multiple instances of the component exists on its game object.
        /// </summary>
        /// <param name="component">The component.</param>
        public static void WarnIfMultipleInstances<TInterface>(this MonoBehaviour component) where TInterface : class
        {
            int counter = 0;
            var components = component.GetComponents<MonoBehaviour>();
            for (int i = 0; i < components.Length; i++)
            {
                var v = components[i] as TInterface;
                if (v != null)
                {
                    counter++;
                }
            }

            if (counter > 1)
            {
                Debug.LogWarning(string.Format("GameObject '{0}' defines multiple component implementing '{1}' which is not recommended.", component.gameObject.name, typeof(TInterface).Name));
            }
        }

        /// <summary>
        /// Determines whether another bounds overlaps this one (and vice versa).
        /// </summary>
        /// <param name="a">This bounds.</param>
        /// <param name="b">The other bounds.</param>
        /// <returns><c>true</c> if they overlap, otherwise false.</returns>
        public static bool Overlaps(this Bounds a, Bounds b)
        {
            if ((b.max.x <= a.min.x) || (b.min.x >= a.max.x))
            {
                return false;
            }

            return ((b.max.z > a.min.z) && (b.min.z < a.max.z));
        }

        /// <summary>
        /// Translates the specified bounds a certain amount.
        /// </summary>
        /// <param name="b">The bounds</param>
        /// <param name="translation">The translation vector.</param>
        /// <returns>The bounds after the translation.</returns>
        public static Bounds Translate(this Bounds b, Vector3 translation)
        {
            b.center = b.center + translation;
            return b;
        }

        /// <summary>
        /// Translates the specified bounds a certain amount.
        /// </summary>
        /// <param name="b">The bounds</param>
        /// <param name="x">The x component of the translation.</param>
        /// <param name="y">The y component of the translation.</param>
        /// <param name="z">The z component of the translation.</param>
        /// <returns>The bounds after the translation.</returns>
        public static Bounds Translate(this Bounds b, float x, float y, float z)
        {
            var center = b.center;
            center.x += x;
            center.y += y;
            center.z += z;
            b.center = center;
            return b;
        }

        /// <summary>
        /// Resizes a bounds by a certain amount.
        /// </summary>
        /// <param name="b">The bounds.</param>
        /// <param name="delta">The delta vector.</param>
        /// <returns>The resized bounds.</returns>
        public static Bounds DeltaSize(this Bounds b, Vector3 delta)
        {
            b.size = b.size + delta;
            return b;
        }

        /// <summary>
        /// Resizes a bounds by a certain amount.
        /// </summary>
        /// <param name="b">The bounds</param>
        /// <param name="dx">The x component of the delta.</param>
        /// <param name="dy">The y component of the delta.</param>
        /// <param name="dz">The z component of the delta.</param>
        /// <returns>The resized bounds.</returns>
        public static Bounds DeltaSize(this Bounds b, float dx, float dy, float dz)
        {
            var size = b.size;
            size.x += dx;
            size.y += dy;
            size.z += dz;
            b.size = size;
            return b;
        }

        /// <summary>
        /// Merges the two bounds
        /// </summary>
        /// <param name="b">The first bounds.</param>
        /// <param name="other">The second bounds.</param>
        /// <returns>A bounds representing the union of the two bounds.</returns>
        public static Bounds Merge(this Bounds b, Bounds other)
        {
            return new Bounds
            {
                min = new Vector3(Mathf.Min(b.min.x, other.min.x), Mathf.Min(b.min.y, other.min.y), Mathf.Min(b.min.z, other.min.z)),
                max = new Vector3(Mathf.Max(b.max.x, other.max.x), Mathf.Max(b.max.y, other.max.y), Mathf.Max(b.max.z, other.max.z)),
            };
        }

        /// <summary>
        /// Get the bounds that represents the intersection of two bounds.
        /// </summary>
        /// <param name="a">The first bounds.</param>
        /// <param name="b">The second bounds.</param>
        /// <returns>The intersection bounds.</returns>
        public static Bounds Intersection(this Bounds a, Bounds b)
        {
            var min = new Vector3(Mathf.Max(a.min.x, b.min.x), Mathf.Max(a.min.y, b.min.y), Mathf.Max(a.min.z, b.min.z));
            var max = new Vector3(Mathf.Min(a.max.x, b.max.x), Mathf.Min(a.max.y, b.max.y), Mathf.Min(a.max.z, b.max.z));

            Bounds res = new Bounds();
            res.SetMinMax(min, max);

            return res;
        }

        /// <summary>
        /// Determines whether one <see cref="Rect"/>. contains another.
        /// </summary>
        /// <param name="rect">The rect to check.</param>
        /// <param name="other">The other rect.</param>
        /// <returns><c>true</c> if <paramref name="other"/> is contained in <paramref name="rect"/>; otherwise <c>false</c></returns>
        public static bool Contains(this Rect rect, Rect other)
        {
            return rect.Contains(other.min) && rect.Contains(other.max);
        }

        /// <summary>
        /// Rounds all members of the specified Rect, producing a Rect with whole number members.
        /// </summary>
        /// <param name="rect">The rect to round.</param>
        /// <returns>The Rect with all members rounded to the nearest whole number.</returns>
        public static Rect Round(this Rect rect)
        {
            rect.xMin = Mathf.Round(rect.xMin);
            rect.xMax = Mathf.Round(rect.xMax);
            rect.yMin = Mathf.Round(rect.yMin);
            rect.yMax = Mathf.Round(rect.yMax);
            return rect;
        }

        /// <summary>
        /// Rounds x and y of the specified Vector2 to the nearest whole number.
        /// </summary>
        /// <param name="v">The vector to round.</param>
        /// <returns>The Vector2 with x and y rounded to the nearest whole number.</returns>
        public static Vector2 Round(this Vector2 v)
        {
            v.x = Mathf.Round(v.x);
            v.y = Mathf.Round(v.y);
            return v;
        }

        /// <summary>
        /// Rounds x, y and z of the specified Vector3 to the nearest whole number.
        /// </summary>
        /// <param name="v">The vector to round.</param>
        /// <returns>The Vector3 with x, y and z rounded to the nearest whole number.</returns>
        public static Vector3 Round(this Vector3 v)
        {
            v.x = Mathf.Round(v.x);
            v.y = Mathf.Round(v.y);
            v.z = Mathf.Round(v.z);
            return v;
        }

        /// <summary>
        /// Adds a component of the specified type if it does not already exist.
        /// </summary>
        /// <typeparam name="T">The type of component to add</typeparam>
        /// <param name="target">The target to which the component will be added.</param>
        /// <param name="entireScene">if set to <c>true</c> the check to see if the component already exists will be done in the entire scene, otherwise it will check the <paramref name="target"/>.</param>
        /// <param name="component">The component regardless of whether it was just added or already existed.</param>
        /// <returns><c>true</c> if the component was added; or <c>false</c> if the component already exists on the game object.</returns>
        public static bool AddIfMissing<T>(this GameObject target, bool entireScene, out T component) where T : Component
        {
            if (entireScene)
            {
                component = ComponentHelper.FindFirstComponentInScene<T>();
            }
            else
            {
                component = target.GetComponent<T>();
            }

            if (component == null)
            {
                component = target.AddComponent<T>();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Adds a component of the specified type if it does not already exist.
        /// </summary>
        /// <typeparam name="T">The type of component to add</typeparam>
        /// <param name="target">The target to which the component will be added.</param>
        /// <param name="entireScene">if set to <c>true</c> the check to see if the component already exists will be done in the entire scene, otherwise it will check the <paramref name="target"/>.</param>
        /// <returns><c>true</c> if the component was added; or <c>false</c> if the component already exists on the game object.</returns>
        public static bool AddIfMissing<T>(this GameObject target, bool entireScene) where T : Component
        {
            T component;
            return AddIfMissing(target, entireScene, out component);
        }

        /// <summary>
        /// Adds a component of the specified type if it does not already exist.
        /// </summary>
        /// <typeparam name="T">The type of component to add</typeparam>
        /// <param name="target">The target to which the component will be added.</param>
        /// <param name="component">The component regardless of whether it was just added or already existed.</param>
        /// <returns><c>true</c> if the component was added; or <c>false</c> if the component already exists on the game object.</returns>
        public static bool AddIfMissing<T>(this GameObject target, out T component) where T : Component
        {
            return AddIfMissing(target, false, out component);
        }

        /// <summary>
        /// Adds a component of the specified type if it does not already exist.
        /// </summary>
        /// <typeparam name="T">The type of component to add</typeparam>
        /// <param name="target">The target to which the component will be added.</param>
        /// <returns><c>true</c> if the component was added; or <c>false</c> if the component already exists on the game object.</returns>
        public static bool AddIfMissing<T>(this GameObject target) where T : Component
        {
            T component;
            return AddIfMissing(target, false, out component);
        }

        /// <summary>
        /// Adds a component but delays its Awake call until it has been configured.
        /// </summary>
        /// <typeparam name="T">The type of component to add</typeparam>
        /// <param name="target">The target to which the component will be added.</param>
        /// <param name="configurator">The action to execute in order to configure the newly added item.</param>
        /// <returns>The component that was added</returns>
        public static T AddComponentSafe<T>(this GameObject target, Action<T> configurator) where T : Component
        {
            target.SetActive(false);
            var c = target.AddComponent<T>();
            configurator(c);
            target.SetActive(true);
            return c;
        }

        /// <summary>
        /// Removes a single component of type T on the specified game object.
        /// </summary>
        /// <typeparam name="T">The type parameter, i.e. what type of component should be removed.</typeparam>
        /// <param name="go">The game object on which the expected component is.</param>
        /// <returns><c>true</c> if it finds and destroys the desired component, <c>false</c> otherwise.</returns>
        public static bool NukeSingle<T>(this GameObject go) where T : Component
        {
            var res = go.GetComponent<T>();
            if (res != null)
            {
                if (Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(res);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(res, true);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Collects a game object and all its descendant game objects in a list.
        /// </summary>
        /// <param name="root">The root game object.</param>
        /// <param name="collector">The list populated by <paramref name="root"/> and all its descendants.</param>
        public static void SelfAndDescendants(this GameObject root, List<GameObject> collector)
        {
            collector.Add(root);

            var t = root.transform;
            int childCount = t.childCount;
            for (int i = 0; i < childCount; i++)
            {
                SelfAndDescendants(t.GetChild(i).gameObject, collector);
            }
        }
    }
}
