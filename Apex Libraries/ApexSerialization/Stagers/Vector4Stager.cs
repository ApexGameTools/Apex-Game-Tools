/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Serialization.Stagers
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Handles staging of <see cref="Vector4"/>s.
    /// </summary>
    /// <seealso cref="Apex.Serialization.IStager" />
    public sealed class Vector4Serializer : IStager
    {
        /// <summary>
        /// Gets the types this stager can handle.
        /// </summary>
        /// <value>
        /// The handled types.
        /// </value>
        public Type[] handledTypes
        {
            get { return new[] { typeof(Vector4) }; }
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
            var val = (Vector4)value;

            return new StageElement(
                name,
                SerializationMaster.ToStageAttribute("x", val.x),
                SerializationMaster.ToStageAttribute("y", val.y),
                SerializationMaster.ToStageAttribute("z", val.z),
                SerializationMaster.ToStageAttribute("w", val.w));
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

            return new Vector4(
                el.AttributeValue<float>("x"),
                el.AttributeValue<float>("y"),
                el.AttributeValue<float>("z"),
                el.AttributeValue<float>("w"));
        }
    }
}
