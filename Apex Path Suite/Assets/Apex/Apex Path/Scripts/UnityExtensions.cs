/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex
{
    using Apex.PathFinding;
    using Apex.Services;
    using Apex.Units;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// Various extension to Unity types.
    /// </summary>
    public static class UnityExtensions
    {
        /// <summary>
        /// Wraps the vector in an IPositioned structure
        /// </summary>
        /// <param name="pos">The position.</param>
        /// <returns>The wrapped position</returns>
        public static IPositioned AsPositioned(this Vector3 pos)
        {
            return new Position(pos);
        }

        /// <summary>
        /// Adjusts an axis.
        /// </summary>
        /// <param name="target">The target to adjust.</param>
        /// <param name="source">The source used for the adjust.</param>
        /// <param name="targetAxis">The target axis.</param>
        /// <returns>The target vector with <paramref name="targetAxis"/> changed to that of <paramref name="source"/></returns>
        public static Vector3 AdjustAxis(this Vector3 target, Vector3 source, Axis targetAxis)
        {
            switch (targetAxis)
            {
                case Axis.Y:
                {
                    target.y = source.y;
                    break;
                }

                case Axis.X:
                {
                    target.x = source.x;
                    break;
                }

                case Axis.Z:
                {
                    target.z = source.z;
                    break;
                }
            }

            return target;
        }

        /// <summary>
        /// Adjusts an axis.
        /// </summary>
        /// <param name="target">The target to adjust.</param>
        /// <param name="value">The adjustment.</param>
        /// <param name="targetAxis">The target axis.</param>
        /// <returns>The target vector with <paramref name="targetAxis"/> changed to <paramref name="value"/></returns>
        public static Vector3 AdjustAxis(this Vector3 target, float value, Axis targetAxis)
        {
            switch (targetAxis)
            {
                case Axis.Y:
                {
                    target.y = value;
                    break;
                }

                case Axis.X:
                {
                    target.x = value;
                    break;
                }

                case Axis.Z:
                {
                    target.z = value;
                    break;
                }
            }

            return target;
        }

        /// <summary>
        /// Gets the unit facade for the unit on which this component resides.
        /// </summary>
        /// <param name="c">The component.</param>
        /// <param name="createIfMissing">Controls whether the facade is created if missing.</param>
        /// <returns>The unit facade for the unit.</returns>
        public static IUnitFacade GetUnitFacade(this Component c, bool createIfMissing = true)
        {
            return GameServices.gameStateManager.GetUnitFacade(c.gameObject, createIfMissing);
        }

        /// <summary>
        /// Gets the unit facade for the unit represented by this game object.
        /// </summary>
        /// <param name="go">The game object.</param>
        /// <param name="createIfMissing">Controls whether the facade is created if missing.</param>
        /// <returns>The unit facade for the unit.</returns>
        public static IUnitFacade GetUnitFacade(this GameObject go, bool createIfMissing = true)
        {
            return GameServices.gameStateManager.GetUnitFacade(go, createIfMissing);
        }

        /// <summary>
        /// Gets the unit facade for the unit represented by this component.
        /// </summary>
        /// <param name="goc">The game object related component.</param>
        /// <param name="createIfMissing">Controls whether the facade is created if missing.</param>
        /// <returns>The unit facade for the unit.</returns>
        public static IUnitFacade GetUnitFacade(this IGameObjectComponent goc, bool createIfMissing = true)
        {
            return GameServices.gameStateManager.GetUnitFacade(goc.gameObject, createIfMissing);
        }

        /// <summary>
        /// Gets a specialized unit facade for the unit on which this component resides.
        /// </summary>
        /// <param name="c">The component.</param>
        /// <param name="createIfMissing">Controls whether the facade is created if missing.</param>
        /// <returns>The unit facade for the unit.</returns>
        public static T GetUnitFacade<T>(this Component c, bool createIfMissing = true) where T : class, IUnitFacade, new()
        {
            return GameServices.gameStateManager.GetUnitFacade<T>(c.gameObject, createIfMissing);
        }

        /// <summary>
        /// Gets a specialized unit facade for the unit on which this component resides.
        /// </summary>
        /// <param name="go">The game object.</param>
        /// <param name="createIfMissing">Controls whether the facade is created if missing.</param>
        /// <returns>The unit facade for the unit.</returns>
        public static T GetUnitFacade<T>(this GameObject go, bool createIfMissing = true) where T : class, IUnitFacade, new()
        {
            return GameServices.gameStateManager.GetUnitFacade<T>(go, createIfMissing);
        }

        /// <summary>
        /// Gets a specialized unit facade for the unit on which this component resides.
        /// </summary>
        /// <param name="goc">The game object related component.</param>
        /// <param name="createIfMissing">Controls whether the facade is created if missing.</param>
        /// <returns>The unit facade for the unit.</returns>
        public static T GetUnitFacade<T>(this IGameObjectComponent goc, bool createIfMissing = true) where T : class, IUnitFacade, new()
        {
            return GameServices.gameStateManager.GetUnitFacade<T>(goc.gameObject, createIfMissing);
        }
    }
}
