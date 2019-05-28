/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using Utilities;

    /// <summary>
    /// Responsible for all things to do with serialization of entities.
    /// </summary>
    public static class SerializationMaster
    {
        private static readonly Dictionary<Type, IValueConverter> _typeConverters = new Dictionary<Type, IValueConverter>();
        private static readonly Dictionary<Type, IStager> _typeStagers = new Dictionary<Type, IStager>();
        private static readonly Type _enumType = typeof(Enum);
        private static readonly Type _stringType = typeof(string);

        private static ISerializer _serializer;
        private static bool _isInitialized;

        [ThreadStatic]
        private static ICollection<IInitializeAfterDeserialization> _initBuffer;

        [ThreadStatic]
        private static ICollection<IInitializeAfterDeserialization> _requiresInit;

        [ThreadStatic]
        private static int _excludeMask;

        /// <summary>
        /// Gets the AI serialized properties of the type.
        /// </summary>
        /// <returns>The serialized properties</returns>
        public static IEnumerable<PropertyInfo> GetSerializedProperties(Type t)
        {
            return from p in t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                   let attrib = p.GetAttribute<ApexSerializationAttribute>(true)
                   where attrib != null && p.CanRead && p.CanWrite
                   select p;
        }

        /// <summary>
        /// Gets the AI serialized fields of the type.
        /// </summary>
        /// <returns>The serialized fields</returns>
        public static IEnumerable<FieldInfo> GetSerializedFields(Type t)
        {
            return from f in t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                   let attrib = f.GetAttribute<ApexSerializationAttribute>(false)
                   where attrib != null
                   select f;
        }

        /// <summary>
        /// Checks if a <see cref="IValueConverter"/> exists for the specified type
        /// </summary>
        /// <param name="forType">The type for which to check.</param>
        /// <returns><c>true</c> if a converter exists; otherwise <c>false</c></returns>
        public static bool ConverterExists(Type forType)
        {
            EnsureInit();
            return _typeConverters.ContainsKey(forType);
        }

        /// <summary>
        /// Checks if a <see cref="IStager"/> exists for the specified type
        /// </summary>
        /// <param name="forType">The type for which to check.</param>
        /// <returns><c>true</c> if a stager exists; otherwise <c>false</c></returns>
        public static bool StagerExists(Type forType)
        {
            EnsureInit();
            return _typeStagers.ContainsKey(forType);
        }

        /// <summary>
        /// Stages a value as an attribute. Requires that a <see cref="IValueConverter"/> exists for the value type.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <returns>A <see cref="StageAttribute"/> representing the value, or <c>null</c> if <paramref name="value"/> is <c>null</c>.</returns>
        public static StageItem ToStageAttribute(string name, object value)
        {
            if (value == null)
            {
                return null;
            }

            return new StageAttribute(name, ToString(value), value is string);
        }

        /// <summary>
        /// Stages a value as a simple value. Requires that a <see cref="IValueConverter"/> exists for the value type.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <returns>A <see cref="StageValue"/> representing the value, or <see cref="StageNull"/> if <paramref name="value"/> is <c>null</c>.</returns>
        public static StageItem ToStageValue(string name, object value)
        {
            if (value == null)
            {
                return new StageNull(name);
            }

            return new StageValue(name, ToString(value), value is string);
        }

        /// <summary>
        /// Converts a value to its string representation. Requires that a <see cref="IValueConverter"/> exists for the value type.
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <returns>The string representation of the value.</returns>
        public static string ToString(object value)
        {
            if (value == null)
            {
                return null;
            }

            var converter = GetConverter(value.GetType());
            if (converter == null)
            {
                throw new ArgumentException("No converter was found for type " + value.GetType());
            }

            return converter.ToString(value);
        }

        /// <summary>
        /// Converts a string to the specified type. Requires that a <see cref="IValueConverter"/> exists for the value type.
        /// </summary>
        /// <typeparam name="T">The type to convert to.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>The converted value.</returns>
        public static T FromString<T>(string value)
        {
            if (value == null)
            {
                return default(T);
            }

            var targetType = typeof(T);
            var converter = GetConverter(targetType);
            if (converter == null)
            {
                throw new ArgumentException("No converter was found for type " + targetType.Name);
            }

            return (T)converter.FromString(value, targetType);
        }

        /// <summary>
        /// Serializes the specified item.
        /// </summary>
        /// <typeparam name="T">The item type</typeparam>
        /// <param name="item">The item.</param>
        /// <param name="pretty">if set to <c>true</c> the serializer will produce a reader friendly string.</param>
        /// <returns>The serialized string representation of the item.</returns>
        public static string Serialize<T>(T item, bool pretty = false)
        {
            EnsureInit();

            var root = Stage(typeof(T).Name, item);
            if (root == null)
            {
                return string.Empty;
            }

            return _serializer.Serialize(root, pretty);
        }

        /// <summary>
        /// Partially serializes the specified item.
        /// </summary>
        /// <typeparam name="T">The item type</typeparam>
        /// <param name="item">The item.</param>
        /// <param name="excludeMask">The mask indicating which properties and fields to exclude, in reference the <see cref="ApexSerializationAttribute.excludeMask"/></param>
        /// <param name="pretty">if set to <c>true</c> the serializer will produce a reader friendly string.</param>
        /// <returns>The serialized string representation of the item.</returns>
        public static string SerializePartial<T>(T item, int excludeMask, bool pretty = false)
        {
            try
            {
                _excludeMask = excludeMask;
                return Serialize(item, pretty);
            }
            finally
            {
                _excludeMask = 0;
            }
        }

        /// <summary>
        /// Serializes the specified stage item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="pretty">if set to <c>true</c> the serializer will produce a reader friendly string.</param>
        /// <returns>The serialized string representation of the item.</returns>
        public static string Serialize(StageElement item, bool pretty = false)
        {
            EnsureInit();
            return _serializer.Serialize(item, pretty);
        }

        /// <summary>
        /// Deserializes the specified data.
        /// </summary>
        /// <typeparam name="T">The type to construct from the data.</typeparam>
        /// <param name="data">The serialized from of the item to construct.</param>
        /// <returns>The item.</returns>
        public static T Deserialize<T>(string data)
        {
            EnsureInit();

            var root = _serializer.Deserialize(data);
            if (root == null)
            {
                return default(T);
            }

            return UnstageAndInitialize<T>(root);
        }

        /// <summary>
        /// Deserializes the specified data.
        /// </summary>
        /// <typeparam name="T">The type to construct from the data.</typeparam>
        /// <param name="data">The serialized from of the item to construct.</param>
        /// <param name="requiresInit">A list that will be populated with references to all entities in the graph that require initialization.</param>
        /// <returns>The item.</returns>
        public static T Deserialize<T>(string data, ICollection<IInitializeAfterDeserialization> requiresInit)
        {
            EnsureInit();

            var root = _serializer.Deserialize(data);
            if (root == null)
            {
                return default(T);
            }

            return Unstage<T>(root, requiresInit);
        }

        /// <summary>
        /// Deserializes the specified data to its staged form.
        /// </summary>
        /// <param name="data">The serialized from of the item.</param>
        /// <returns>The staged form of the item.</returns>
        public static StageElement Deserialize(string data)
        {
            EnsureInit();
            return _serializer.Deserialize(data) as StageElement;
        }

        /// <summary>
        /// Stages a value. This is intended for use by <see cref="IStager"/>s.
        /// </summary>
        /// <param name="name">The name of the value.</param>
        /// <param name="value">The value.</param>
        /// <returns>The staged form of the value.</returns>
        public static StageItem Stage(string name, object value)
        {
            if (value == null)
            {
                return new StageNull(name);
            }

            var valueType = value.GetType();

            var forPrepare = value as IPrepareForSerialization;
            if (forPrepare != null)
            {
                forPrepare.Prepare();
            }

            var stager = GetStager(valueType);
            if (stager != null)
            {
                return stager.StageValue(name, value);
            }

            var converter = GetConverter(valueType);
            if (converter != null)
            {
                return new StageValue(name, converter.ToString(value), valueType == _stringType);
            }

            return ReflectOut(name, value);
        }

        /// <summary>
        /// Unstages and initialize the staged item.
        /// </summary>
        /// <typeparam name="T">The type of the item to construct from the staged form.</typeparam>
        /// <param name="item">The staged item.</param>
        /// <returns>The item.</returns>
        public static T UnstageAndInitialize<T>(StageItem item)
        {
            if (_initBuffer == null)
            {
                _initBuffer = new List<IInitializeAfterDeserialization>();
            }
            else
            {
                _initBuffer.Clear();
            }

            var root = Unstage<T>(item, _initBuffer);
            if (_initBuffer.Count > 0)
            {
                foreach (var doInit in _initBuffer)
                {
                    doInit.Initialize(root);
                }

                _initBuffer.Clear();
            }

            return root;
        }

        /// <summary>
        /// Unstages the specified staged item.
        /// </summary>
        /// <typeparam name="T">The type of the item to construct from the staged form.</typeparam>
        /// <param name="item">The staged item.</param>
        /// <param name="requiresInit">A list that will be populated with references to all entities in the graph that require initialization.</param>
        /// <returns>The item.</returns>
        /// <exception cref="System.InvalidOperationException">If called during a nested unstage operation.</exception>
        public static T Unstage<T>(StageItem item, ICollection<IInitializeAfterDeserialization> requiresInit)
        {
            //We make use of a thread static var since we cannot pass it through the call hierarchy as some stagers may call methods on here as well.
            if (_requiresInit != null)
            {
                throw new InvalidOperationException("Generic overloads of Unstage cannot be called during a nested unstage operation.");
            }

            _requiresInit = requiresInit;

            try
            {
                var result = Unstage(item, typeof(T));

                if (result == null)
                {
                    return default(T);
                }

                return (T)result;
            }
            finally
            {
                _requiresInit = null;
            }
        }

        /// <summary>
        /// Unstages the specified staged item. This is intended for use by <see cref="IStager"/>s.
        /// </summary>
        /// <param name="item">The staged item.</param>
        /// <typeparam name="T">The type of the unstaged value.</typeparam>
        /// <returns>The unstaged value</returns>
        /// <exception cref="System.Runtime.Serialization.SerializationException">
        /// If no <see cref="IStager"/> or <see cref="IValueConverter"/> matching <paramref name="targetType"/> was found
        /// or
        /// If the element is not supported, e.g. a custom derivative of <see cref="StageItem"/>.
        /// </exception>
        public static T Unstage<T>(StageItem item)
        {
            return (T)Unstage(item, typeof(T));
        }

        /// <summary>
        /// Unstages the specified staged item. This is intended for use by <see cref="IStager"/>s.
        /// </summary>
        /// <param name="item">The staged item.</param>
        /// <param name="targetType">Type of the item.</param>
        /// <returns>The unstaged value</returns>
        /// <exception cref="System.Runtime.Serialization.SerializationException">
        /// If no <see cref="IStager"/> or <see cref="IValueConverter"/> matching <paramref name="targetType"/> was found
        /// or
        /// If the element is not supported, e.g. a custom derivative of <see cref="StageItem"/>.
        /// </exception>
        public static object Unstage(StageItem item, Type targetType)
        {
            if (item is StageNull)
            {
                return null;
            }

            var stager = GetStager(targetType);
            if (stager != null)
            {
                return stager.UnstageValue(item, targetType);
            }

            var valueItem = item as StageValue;
            if (valueItem != null)
            {
                var converter = GetConverter(targetType);
                if (converter != null)
                {
                    return converter.FromString(valueItem.value, targetType);
                }
                else
                {
                    throw new SerializationException("Unable to unstage, no converter or stager was found for type: " + targetType.Name);
                }
            }

            var elementItem = item as StageElement;
            if (elementItem == null)
            {
                throw new SerializationException("Unable to unstage, the element is not supported: " + targetType.Name);
            }

            return ReflectIn(elementItem);
        }

        private static bool TryStage(string name, object value, out StageItem result)
        {
            result = Stage(name, value);
            return (result != null);
        }

        private static bool TryUnstage(StageItem item, Type targetType, out object value)
        {
            value = Unstage(item, targetType);
            return (value != null);
        }

        private static StageElement ReflectOut(string elementName, object item)
        {
            var itemType = item.GetType();
            var nameSplit = itemType.AssemblyQualifiedName.Split(',');
            var element = new StageElement(elementName, new StageAttribute("type", string.Concat(nameSplit[0], ",", nameSplit[1]), true));

            var properties = from p in itemType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                             let attrib = p.GetAttribute<ApexSerializationAttribute>(true)
                             where attrib != null && p.CanRead && p.CanWrite && (attrib.excludeMask & _excludeMask) == 0
                             select new AIPropInfo
                             {
                                 prop = p,
                                 defaultValue = attrib.defaultValue
                             };

            foreach (var p in properties)
            {
                var val = p.prop.GetValue(item, null);
                if (val != null && !val.Equals(p.defaultValue))
                {
                    StageItem propElement;
                    if (TryStage(p.prop.Name, val, out propElement))
                    {
                        element.Add(propElement);
                    }
                }
            }

            var fields = from f in itemType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                         let attrib = f.GetAttribute<ApexSerializationAttribute>(false)
                         where attrib != null && (attrib.excludeMask & _excludeMask) == 0
                         select new AIFieldInfo
                         {
                             field = f,
                             defaultValue = attrib.defaultValue
                         };

            foreach (var f in fields)
            {
                var val = f.field.GetValue(item);
                if (val != null && !val.Equals(f.defaultValue))
                {
                    StageItem propElement;
                    if (TryStage(f.field.Name, val, out propElement))
                    {
                        element.Add(propElement);
                    }
                }
            }

            return element;
        }

        private static object ReflectIn(StageElement element)
        {
            var typeName = element.AttributeValueOrDefault<string>("type", null);
            if (typeName == null)
            {
                throw new SerializationException("Invalid structure detected, missing type info.");
            }

            var itemType = Type.GetType(typeName, true);
            object instance;

            try
            {
                instance = Activator.CreateInstance(itemType, true);
            }
            catch (MissingMethodException mme)
            {
                throw new SerializationException(string.Format("Unable to create type {0}, ensure it has a parameterless constructor", itemType.Name), mme);
            }

            var forInit = instance as IInitializeAfterDeserialization;
            if (forInit != null)
            {
                if (_requiresInit == null)
                {
                    throw new InvalidOperationException("An entity requires initialization but was unable to register, call UnstageAndInitialize instead.");
                }

                _requiresInit.Add(forInit);
            }

            var properties = GetSerializedProperties(itemType);

            foreach (var p in properties)
            {
                var member = element.Item(p.Name);
                if (member == null)
                {
                    continue;
                }

                object val;
                if (TryUnstage(member, p.PropertyType, out val))
                {
                    p.SetValue(instance, val, null);
                }
            }

            var fields = GetSerializedFields(itemType);

            foreach (var f in fields)
            {
                var member = element.Item(f.Name);
                if (member == null)
                {
                    continue;
                }

                object val;
                if (TryUnstage(member, f.FieldType, out val))
                {
                    f.SetValue(instance, val);
                }
            }

            return instance;
        }

        private static void EnsureInit()
        {
            if (!_isInitialized)
            {
                lock (_typeConverters)
                {
                    if (!_isInitialized)
                    {
                        PopulateKnownSerializers();
                        _isInitialized = true;
                    }
                }
            }
        }

        private static IStager GetStager(Type forType)
        {
            EnsureInit();

            if (forType.IsGenericType)
            {
                forType = forType.GetGenericTypeDefinition();
            }
            else if (forType.IsArray)
            {
                forType = typeof(Array);
            }

            IStager result = null;
            _typeStagers.TryGetValue(forType, out result);

            return result;
        }

        private static IValueConverter GetConverter(Type forType)
        {
            EnsureInit();

            if (forType.IsGenericType)
            {
                forType = forType.GetGenericTypeDefinition();
            }
            else if (forType.IsEnum)
            {
                forType = _enumType;
            }

            IValueConverter result = null;
            _typeConverters.TryGetValue(forType, out result);

            return result;
        }

        private static void PopulateKnownSerializers()
        {
            var relevantTypes = ApexReflection.GetRelevantTypes();

            var serializerTypes = from t in relevantTypes
                                  where (typeof(IValueConverter).IsAssignableFrom(t) || typeof(IStager).IsAssignableFrom(t) || typeof(ISerializer).IsAssignableFrom(t)) &&
                                  !t.IsAbstract && t.GetConstructor(Type.EmptyTypes) != null
                                  select t;

            foreach (var st in serializerTypes)
            {
                object instance;
                var singletonField = st.GetField("instance", BindingFlags.Public | BindingFlags.Static);
                if (singletonField != null)
                {
                    instance = singletonField.GetValue(null);
                }
                else
                {
                    instance = Activator.CreateInstance(st);
                }

                var vc = instance as IValueConverter;
                if (vc != null)
                {
                    if (vc.handledTypes == null)
                    {
                        continue;
                    }

                    foreach (var t in vc.handledTypes)
                    {
                        if (!_typeConverters.ContainsKey(t) || st.IsDefined<SerializationOverrideAttribute>(false))
                        {
                            _typeConverters[t] = vc;
                        }
                    }
                }
                else if (instance is ISerializer && (_serializer == null || st.IsDefined<SerializationOverrideAttribute>(false)))
                {
                    _serializer = (ISerializer)instance;
                }
                else
                {
                    var s = instance as IStager;

                    if (s.handledTypes == null)
                    {
                        continue;
                    }

                    foreach (var t in s.handledTypes)
                    {
                        if (!_typeStagers.ContainsKey(t) || st.IsDefined<SerializationOverrideAttribute>(false))
                        {
                            _typeStagers[t] = s;
                        }
                    }
                }
            }
        }

        private struct AIPropInfo
        {
            internal PropertyInfo prop;
            internal object defaultValue;
        }

        private struct AIFieldInfo
        {
            internal FieldInfo field;
            internal object defaultValue;
        }
    }
}
