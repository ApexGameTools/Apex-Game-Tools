/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Serialization.Stagers
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Handles staging of <see cref="Bounds"/> structures.
    /// </summary>
    /// <seealso cref="Apex.Serialization.IStager" />
    public sealed class BoundsStager : IStager
    {
        /// <summary>
        /// Gets the types this stager can handle.
        /// </summary>
        /// <value>
        /// The handled types.
        /// </value>
        public Type[] handledTypes
        {
            get { return new[] { typeof(Bounds) }; }
        }

        /// <summary>
        /// Stages the value.
        /// </summary>
        /// <param name="name">The name to give the resulting <see cref="StageItem" />.</param>
        /// <param name="value">The value to stage</param>
        /// <returns>
        /// The element containing the staged value.
        /// </returns>
        public StageItem StageValue(string name, object value)
        {
            var val = (Bounds)value;

            return new StageElement(
                name,
                SerializationMaster.ToStageAttribute("center.x", val.center.x),
                SerializationMaster.ToStageAttribute("center.y", val.center.y),
                SerializationMaster.ToStageAttribute("center.z", val.center.z),
                SerializationMaster.ToStageAttribute("size.x", val.size.x),
                SerializationMaster.ToStageAttribute("size.y", val.size.y),
                SerializationMaster.ToStageAttribute("size.z", val.size.z));
        }

        /// <summary>
        /// Unstages the value.
        /// </summary>
        /// <param name="item">The stage item to unstage.</param>
        /// <param name="targetType">Type of the value.</param>
        /// <returns>
        /// The unstaged value.
        /// </returns>
        public object UnstageValue(StageItem item, Type targetType)
        {
            var el = (StageElement)item;

            return new Bounds(
                new Vector3(
                    SerializationMaster.FromString<float>(el.Attribute("center.x").value),
                    SerializationMaster.FromString<float>(el.Attribute("center.y").value),
                    SerializationMaster.FromString<float>(el.Attribute("center.z").value)),
                new Vector3(
                    SerializationMaster.FromString<float>(el.Attribute("size.x").value),
                    SerializationMaster.FromString<float>(el.Attribute("size.y").value),
                    SerializationMaster.FromString<float>(el.Attribute("size.z").value)));
        }
    }
}