/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Serialization
{
    using System;

    /// <summary>
    /// Various extensions related to serialization
    /// </summary>
    public static class SerializationExtensions
    {
        /// <summary>
        /// Gets the converted value of the <see cref="StageItem"/> or a default if the element is null or has no value.
        /// </summary>
        /// <param name="item">The stage item.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value of the item, or <paramref name="defaultValue"/> if <paramref name="item"/> is <c>null</c> or has no value.</returns>
        public static T ValueOrDefault<T>(this StageItem item, T defaultValue = default(T))
        {
            if (item == null || item is StageNull)
            {
                return defaultValue;
            }

            if (item is StageContainer)
            {
                return SerializationMaster.UnstageAndInitialize<T>(item);
            }

            var val = (StageValue)item;
            return SerializationMaster.FromString<T>(val.value);
        }

        /// <summary>
        /// Gets the converted value of the specified item, or a default value if the element or the item is null or if it has no value.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="itemName">Name of the item.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value of the item, or <paramref name="defaultValue"/> if <paramref name="element"/> or the item is <c>null</c> or has no value.</returns>
        public static T ValueOrDefault<T>(this StageElement element, string itemName, T defaultValue = default(T))
        {
            if (element == null)
            {
                return defaultValue;
            }

            return ValueOrDefault<T>(element.Item(itemName), defaultValue);
        }

        /// <summary>
        /// Gets the converted value of the specified item.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="itemName">Name of the value.</param>
        /// <returns>The value of the item.</returns>
        /// <exception cref="System.ArgumentException">If no value item by that name was found.</exception>
        public static T Value<T>(this StageElement element, string itemName)
        {
            if (element != null)
            {
                var item = element.Item(itemName);
                if (item != null)
                {
                    return ValueOrDefault<T>(item);
                }
            }

            throw new ArgumentException("No item by that name was found: " + itemName);
        }

        /// <summary>
        /// Gets the converted value of the specified attribute, or a default value if the element or the attribute is null or if it has no value.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value of the attribute, or <paramref name="defaultValue"/> if <paramref name="element"/> or the attribute is <c>null</c> or has no value.</returns>
        public static T AttributeValueOrDefault<T>(this StageElement element, string attributeName, T defaultValue = default(T))
        {
            if (element == null)
            {
                return defaultValue;
            }

            var attrib = element.Attribute(attributeName);
            if (attrib == null)
            {
                return defaultValue;
            }

            return SerializationMaster.FromString<T>(attrib.value);
        }

        /// <summary>
        /// Gets the converted value of the specified attribute.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <returns>The value of the attribute.</returns>
        /// <exception cref="System.ArgumentException">If no attribute by that name was found.</exception>
        public static T AttributeValue<T>(this StageElement element, string attributeName)
        {
            if (element != null)
            {
                var attrib = element.Attribute(attributeName);
                if (attrib != null)
                {
                    return SerializationMaster.FromString<T>(attrib.value);
                }
            }

            throw new ArgumentException("No attribute by that name was found: " + attributeName);
        }

        /// <summary>
        /// Adds an item with a value.
        /// </summary>
        /// <param name="parent">The parent element to add to.</param>
        /// <param name="name">The name of the item to add.</param>
        /// <param name="value">The value of the item to add.</param>
        /// <param name="onlyIfNotNull">if set to <c>true</c> no item is added if the <paramref name="value"/> is <c>null</c>.</param>
        public static void AddValue(this StageContainer parent, string name, object value, bool onlyIfNotNull = true)
        {
            if (onlyIfNotNull && value == null)
            {
                return;
            }

            parent.Add(SerializationMaster.Stage(name, value));
        }

        /// <summary>
        /// Adds a text element.
        /// </summary>
        /// <param name="parent">The parent element to add to.</param>
        /// <param name="name">The name of the item to add.</param>
        /// <param name="value">The value of the item to add.</param>
        /// <param name="onlyIfNotNullOrEmpty">if set to <c>true</c> no item is added if the <paramref name="value"/> is <c>null</c> or empty.</param>
        public static void AddTextValue(this StageContainer parent, string name, string value, bool onlyIfNotNullOrEmpty = true)
        {
            if (onlyIfNotNullOrEmpty && string.IsNullOrEmpty(value))
            {
                return;
            }

            parent.Add(SerializationMaster.ToStageValue(name, value));
        }

        /// <summary>
        /// Sets the value of a value element. If it does not exist it is created.
        /// If multiple identically named items exists only the first one is set.
        /// </summary>
        /// <param name="parent">The parent element.</param>
        /// <param name="name">The name of the item.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="removeIfNull">If <c>true</c> and <para<paramref name="value"/> is <c>null</c>, the item is removed rather than converted to a <see cref="StageNull"/></param>
        public static void SetValue(this StageElement parent, string name, object value, bool removeIfNull = true)
        {
            bool remove = (removeIfNull && (value == null));

            var item = parent.Item(name);
            if (item == null)
            {
                if (!remove)
                {
                    parent.Add(SerializationMaster.ToStageValue(name, value));
                }

                return;
            }

            if (item is StageAttribute)
            {
                throw new InvalidOperationException("Use SetTextAttribute to set text attributes.");
            }

            var nullItem = item as StageNull;
            if (item != null)
            {
                if (value != null)
                {
                    nullItem.Remove();
                    parent.Add(SerializationMaster.ToStageValue(name, value));
                }

                return;
            }

            var valueItem = item as StageValue;
            if (item == null)
            {
                throw new InvalidOperationException("Only value elements can be set using this method.");
            }

            if (remove)
            {
                item.Remove();
            }
            else if (value == null)
            {
                item.Remove();
                parent.Add(new StageNull(name));
            }
            else if (valueItem.isText && !(value is string))
            {
                throw new InvalidOperationException("Use SetTextValue to set text values.");
            }
            else
            {
                valueItem.value = SerializationMaster.ToString(value);
            }
        }

        /// <summary>
        /// Sets the value of a text element. If it does not exist it is created.
        /// If multiple identically named items exists only the first one is set.
        /// </summary>
        /// <param name="parent">The parent element.</param>
        /// <param name="name">The name of the item.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="removeIfNullOrEmpty">If <c>true</c> and <para<paramref name="value"/> is <c>null</c> or empty, the item is removed rather than converted to a <see cref="StageNull"/></param>
        public static void SetTextValue(this StageElement parent, string name, string value, bool removeIfNullOrEmpty = true)
        {
            bool remove = (removeIfNullOrEmpty && string.IsNullOrEmpty(value));

            var item = parent.Item(name);
            if (item == null)
            {
                if (!remove)
                {
                    parent.Add(SerializationMaster.ToStageValue(name, value));
                }

                return;
            }

            if (item is StageAttribute)
            {
                throw new InvalidOperationException("Use SetTextAttribute to set text attributes.");
            }

            var nullItem = item as StageNull;
            if (item != null)
            {
                if (value != null)
                {
                    nullItem.Remove();
                    parent.Add(SerializationMaster.ToStageValue(name, value));
                }

                return;
            }

            var valueItem = item as StageValue;
            if (item == null)
            {
                throw new InvalidOperationException("Only value elements can be set using this method.");
            }

            if (remove)
            {
                item.Remove();
            }
            else if (value == null)
            {
                item.Remove();
                parent.Add(new StageNull(name));
            }
            else if (!valueItem.isText)
            {
                throw new InvalidOperationException("Cannot set a text value on a non-text value item.");
            }
            else
            {
                valueItem.value = SerializationMaster.ToString(value);
            }
        }

        /// <summary>
        /// Adds an attribute with a value.
        /// </summary>
        /// <param name="parent">The parent element to add to.</param>
        /// <param name="name">The name of the attribute to add.</param>
        /// <param name="value">The value of the attribute to add.</param>
        /// <param name="onlyIfNotNull">if set to <c>true</c> no attribute is added if the <paramref name="value"/> is <c>null</c>.</param>
        public static void AddAttribute(this StageElement parent, string name, object value, bool onlyIfNotNull = true)
        {
            if (onlyIfNotNull && value == null)
            {
                return;
            }

            parent.Add(SerializationMaster.ToStageAttribute(name, value));
        }

        /// <summary>
        /// Adds an attribute with a text value.
        /// </summary>
        /// <param name="parent">The parent element to add to.</param>
        /// <param name="name">The name of the attribute to add.</param>
        /// <param name="value">The value of the attribute to add.</param>
        /// <param name="onlyIfNotNullOrEmpty">if set to <c>true</c> no attribute is added if the <paramref name="value"/> is <c>null</c> or empty.</param>
        public static void AddTextAttribute(this StageElement parent, string name, string value, bool onlyIfNotNullOrEmpty = true)
        {
            if (onlyIfNotNullOrEmpty && string.IsNullOrEmpty(value))
            {
                return;
            }

            parent.Add(SerializationMaster.ToStageAttribute(name, value));
        }

        /// <summary>
        /// Sets the value of an attribute. If it does not exist it is created.
        /// If the value is <c>null</c> the attribute is removed.
        /// If multiple identically named attributes exists only the first one is set.
        /// </summary>
        /// <param name="parent">The parent element.</param>
        /// <param name="name">The name of the attribute.</param>
        /// <param name="value">The value to set.</param>
        public static void SetAttribute(this StageElement parent, string name, object value)
        {
            bool remove = (value == null);

            var attrib = parent.Attribute(name);
            if (attrib == null)
            {
                if (!remove)
                {
                    parent.Add(SerializationMaster.ToStageAttribute(name, value));
                }
            }
            else if (remove)
            {
                attrib.Remove();
            }
            else if (attrib.isText && !(value is string))
            {
                throw new InvalidOperationException("Use SetTextAttribute to set text attributes.");
            }
            else
            {
                attrib.value = SerializationMaster.ToString(value);
            }
        }

        /// <summary>
        /// Sets the value of a text attribute. If it does not exist it is created.
        /// If the value is <c>null</c> the attribute is removed.
        /// If multiple identically named attributes exists only the first one is set.
        /// </summary>
        /// <param name="parent">The parent element.</param>
        /// <param name="name">The name of the attribute.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="removeIfEmpty">if set to <c>true</c> the attribute is removed if the <paramref name="value"/> is empty. The attribute is always removed if <paramref name="value"/> is <c>null</c></param>
        public static void SetTextAttribute(this StageElement parent, string name, string value, bool removeIfEmpty = true)
        {
            bool remove = (value == null) || (removeIfEmpty && string.IsNullOrEmpty(value));

            var attrib = parent.Attribute(name);
            if (attrib == null)
            {
                if (!remove)
                {
                    parent.Add(SerializationMaster.ToStageAttribute(name, value));
                }
            }
            else if (remove)
            {
                attrib.Remove();
            }
            else if (!attrib.isText)
            {
                throw new InvalidOperationException("Cannot set a text value on a non-text attribute.");
            }
            else
            {
                attrib.value = SerializationMaster.ToString(value);
            }
        }
    }
}
