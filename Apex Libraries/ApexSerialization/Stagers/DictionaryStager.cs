/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Serialization.Stagers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Handles staging of <see cref="Dictionary{TKey,TValue}"/>s.
    /// </summary>
    /// <seealso cref="Apex.Serialization.IStager" />
    public sealed class DictionaryStager : IStager
    {
        /// <summary>
        /// Gets the types this stager can handle.
        /// </summary>
        /// <value>
        /// The handled types.
        /// </value>
        public Type[] handledTypes
        {
            get { return new[] { typeof(Dictionary<,>) }; }
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
            var dic = value as IDictionary;
            var count = dic.Count;
            var listElement = new StageList(name);

            foreach (var key in dic.Keys)
            {
                var item = new StageElement(
                    string.Empty,
                    SerializationMaster.Stage("key", key),
                    SerializationMaster.Stage("value", dic[key]));

                listElement.Add(item);
            }

            return listElement;
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
            var el = (StageList)item;

            var elements = el.Elements().ToArray();

            var typeArgs = targetType.GetGenericArguments();
            Type keyType = typeArgs[0];
            Type valueType = typeArgs[1];

            var dic = Activator.CreateInstance(targetType, elements.Length) as IDictionary;
            for (int i = 0; i < elements.Length; i++)
            {
                var element = elements[i];
                var key = SerializationMaster.Unstage(element.Item("key"), keyType);
                var value = SerializationMaster.Unstage(element.Item("value"), valueType);
                dic.Add(key, value);
            }

            return dic;
        }
    }
}
