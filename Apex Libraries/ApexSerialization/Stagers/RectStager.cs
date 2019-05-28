/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Serialization.Stagers
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Handles staging of <see cref="Rect"/>s.
    /// </summary>
    /// <seealso cref="Apex.Serialization.IStager" />
    public sealed class RectSerializer : IStager
    {
        /// <summary>
        /// Gets the types this stager can handle.
        /// </summary>
        /// <value>
        /// The handled types.
        /// </value>
        public Type[] handledTypes
        {
            get { return new[] { typeof(Rect) }; }
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
            var val = (Rect)value;

            return new StageElement(
                name,
                SerializationMaster.ToStageAttribute("left", val.xMin),
                SerializationMaster.ToStageAttribute("top", val.yMin),
                SerializationMaster.ToStageAttribute("width", val.width),
                SerializationMaster.ToStageAttribute("height", val.height));
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

            return new Rect(
                el.AttributeValue<float>("left"),
                el.AttributeValue<float>("top"),
                el.AttributeValue<float>("width"),
                el.AttributeValue<float>("height"));
        }
    }
}
