/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Serialization.Stagers
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Handles staging of <see cref="Color"/>s.
    /// </summary>
    /// <seealso cref="Apex.Serialization.IStager" />
    public sealed class ColorSerializer : IStager
    {
        /// <summary>
        /// Gets the types this stager can handle.
        /// </summary>
        /// <value>
        /// The handled types.
        /// </value>
        public Type[] handledTypes
        {
            get { return new[] { typeof(Color) }; }
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
            var val = (Color)value;

            return new StageElement(
                name,
                SerializationMaster.ToStageAttribute("r", val.r),
                SerializationMaster.ToStageAttribute("g", val.g),
                SerializationMaster.ToStageAttribute("b", val.b),
                SerializationMaster.ToStageAttribute("a", val.a));
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

            return new Color(
                el.AttributeValue<float>("r"),
                el.AttributeValue<float>("g"),
                el.AttributeValue<float>("b"),
                el.AttributeValue<float>("a"));
        }
    }
}
