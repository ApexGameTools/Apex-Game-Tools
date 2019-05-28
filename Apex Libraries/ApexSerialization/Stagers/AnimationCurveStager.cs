/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Serialization.Stagers
{
    using System;
    using System.Linq;
    using UnityEngine;

    /// <summary>
    /// Handles staging of <see cref="AnimationCurve"/>s.
    /// </summary>
    /// <seealso cref="Apex.Serialization.IStager" />
    public sealed class AnimationCurveSerializer : IStager
    {
        /// <summary>
        /// Gets the types this stager can handle.
        /// </summary>
        /// <value>
        /// The handled types.
        /// </value>
        public Type[] handledTypes
        {
            get { return new[] { typeof(AnimationCurve) }; }
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
            var val = (AnimationCurve)value;
            var keys = val.keys;

            var curveElement = new StageElement(name);
            for (int i = 0; i < keys.Length; i++)
            {
                var key = keys[i];
                var item = new StageElement(
                    "key",
                    SerializationMaster.ToStageAttribute("time", key.time),
                    SerializationMaster.ToStageAttribute("value", key.value),
                    SerializationMaster.ToStageAttribute("inTangent", key.inTangent),
                    SerializationMaster.ToStageAttribute("outTangent", key.outTangent),
                    SerializationMaster.ToStageAttribute("tangentMode", key.tangentMode));

                curveElement.Add(item);
            }

            return curveElement;
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
            var element = (StageElement)item;
            var keys = (from key in element.Elements()
                        select new Keyframe
                        {
                            time = key.AttributeValue<float>("time"),
                            value = key.AttributeValue<float>("value"),
                            inTangent = key.AttributeValue<float>("inTangent"),
                            outTangent = key.AttributeValue<float>("outTangent"),
                            tangentMode = key.AttributeValue<int>("tangentMode")
                        }).ToArray();

            return new AnimationCurve(keys);
        }
    }
}