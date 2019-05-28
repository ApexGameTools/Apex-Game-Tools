/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Serialization.Stagers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Handles staging of <see cref="List{T}"/>s.
    /// </summary>
    /// <seealso cref="Apex.Serialization.IStager" />
    public sealed class ListStager : IStager
    {
        /// <summary>
        /// Gets the types this stager can handle.
        /// </summary>
        /// <value>
        /// The handled types.
        /// </value>
        public Type[] handledTypes
        {
            get { return new[] { typeof(List<>), typeof(Array) }; }
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
            var list = value as IList;
            var count = list.Count;
            var listElement = new StageList(name);

            for (int i = 0; i < count; i++)
            {
                var item = SerializationMaster.Stage("Item", list[i]);
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

            var items = el.Items().ToArray();

            Type itemType;
            if (targetType.IsArray)
            {
                itemType = targetType.GetElementType();

                var arr = Array.CreateInstance(itemType, items.Length);
                for (int i = 0; i < items.Length; i++)
                {
                    var itemValue = SerializationMaster.Unstage(items[i], itemType);
                    arr.SetValue(itemValue, i);
                }

                return arr;
            }

            //A list
            if (targetType.IsGenericType)
            {
                itemType = targetType.GetGenericArguments()[0];
            }
            else
            {
                itemType = typeof(object);
            }

            var list = Activator.CreateInstance(targetType, items.Length) as IList;
            for (int i = 0; i < items.Length; i++)
            {
                var itemValue = SerializationMaster.Unstage(items[i], itemType);
                list.Add(itemValue);
            }

            return list;
        }
    }
}
