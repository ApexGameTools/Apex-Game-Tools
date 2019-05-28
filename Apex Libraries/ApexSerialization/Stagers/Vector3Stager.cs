/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Serialization.Stagers
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Handles serialization of <see cref="Vector3"/>s.
    /// </summary>
    /// <seealso cref="Apex.Serialization.IStager" />
    public sealed class Vector3Serializer : IStager
    {
        /// <summary>
        /// Gets the types this stager can handle.
        /// </summary>
        /// <value>
        /// The handled types.
        /// </value>
        public Type[] handledTypes
        {
            get { return new[] { typeof(Vector3) }; }
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
            var val = (Vector3)value;

            return new StageElement(
                name,
                SerializationMaster.ToStageAttribute("x", val.x),
                SerializationMaster.ToStageAttribute("y", val.y),
                SerializationMaster.ToStageAttribute("z", val.z));
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

            return new Vector3(
                el.AttributeValue<float>("x"),
                el.AttributeValue<float>("y"),
                el.AttributeValue<float>("z"));
        }
    }
}
