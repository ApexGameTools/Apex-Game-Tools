/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Editor.Reflection
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using Apex.Editor;
    using UnityEditor;
    using UnityEngine;

    [TypesHandled(typeof(Dictionary<,>))]
    public class DictionaryField : IEditorField
    {
        private object _owner;
        private IDictionary _dictionary;
        private Type _keyType;
        private Type _valueType;
        private Action<IDictionary> _setter;
        private List<EditorItem> _editorItems;

        private GUIContent _label;
        private ItemConstructor _itemCreator;

        public DictionaryField(MemberData data, object owner)
        {
            _owner = owner;
            _label = new GUIContent(data.name, data.description);
            this.memberName = data.member.Name;

            var prop = data.member as PropertyInfo;
            if (prop != null)
            {
                GetKeyValueTypes(prop.PropertyType);
                _dictionary = (IDictionary)prop.GetValue(owner, null);
                if (_dictionary == null)
                {
                    _setter = (dict) =>
                    {
                        _dictionary = dict;
                        prop.SetValue(owner, dict, null);
                        if (_itemCreator != null)
                        {
                            _itemCreator.SetParent(_dictionary);
                        }
                    };
                }
            }
            else
            {
                var field = data.member as FieldInfo;
                if (field != null)
                {
                    GetKeyValueTypes(field.FieldType);
                    _dictionary = (IDictionary)field.GetValue(owner);
                    if (_dictionary == null)
                    {
                        _setter = (dict) =>
                        {
                            _dictionary = dict;
                            field.SetValue(owner, dict);
                            if (_itemCreator != null)
                            {
                                _itemCreator.SetParent(_dictionary);
                            }
                        };
                    }
                }
                else
                {
                    throw new ArgumentException("Invalid reflected member type, only fields and properties are supported.");
                }
            }

            if (_dictionary != null)
            {
                var count = _dictionary.Count;
                _editorItems = new List<EditorItem>(count);
                _itemCreator = new ItemConstructor(_dictionary, _keyType, _valueType);

                foreach (var key in _dictionary.Keys)
                {
                    var item = _itemCreator.Conctruct(key, _dictionary[key]);
                    _editorItems.Add(ReflectMaster.Reflect(item));
                }
            }
            else
            {
                _editorItems = new List<EditorItem>();
            }
        }

        public string memberName
        {
            get;
            private set;
        }

        public object currentValue
        {
            get { return _dictionary; }
        }

        private void GetKeyValueTypes(Type dictType)
        {
            _keyType = dictType.GetGenericArguments()[0];
            _valueType = dictType.GetGenericArguments()[1];
        }

        public void RenderField(AIInspectorState state)
        {
            var evt = Event.current;
            var mousePos = evt.mousePosition;
            var boxStyle = new GUIStyle("Box");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(_label);
            EditorGUILayout.Separator();
            if (GUILayout.Button(SharedStyles.addTooltip, SharedStyles.BuiltIn.addButtonSmall))
            {
                AddNew(mousePos, state);
            }

            EditorGUILayout.EndHorizontal();

            var count = _editorItems.Count;
            if (count == 0)
            {
                return;
            }

            EditorGUILayout.BeginVertical(boxStyle);

            var removeIdx = -1;
            for (int i = 0; i < count; i++)
            {
                EditorGUILayout.BeginVertical();
                if (DrawEditorItem(_editorItems[i], state))
                {
                    removeIdx = i;
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();

            if (removeIdx >= 0 && DisplayHelper.ConfirmDelete("item"))
            {
                var item = (IKeyedItem)_editorItems[removeIdx].item;
                _dictionary.Remove(item.key);
                _editorItems.RemoveAt(removeIdx);
                state.MarkDirty();
            }
        }

        private bool DrawEditorItem(EditorItem item, AIInspectorState state)
        {
            var result = false;

            // Item Header
            EditorGUILayout.BeginHorizontal(SharedStyles.BuiltIn.listItemHeader);

            EditorGUILayout.LabelField(item.name, SharedStyles.BuiltIn.normalText);

            if (GUILayout.Button(SharedStyles.deleteTooltip, SharedStyles.BuiltIn.deleteButtonSmall))
            {
                result = true;

                // We do not want the button click itself to count as a change. Since no other changes are expected when a button click is encountered, we can just set it to false.
                GUI.changed = false;
            }

            EditorGUILayout.EndHorizontal();

            // Item fields
            item.Render(state);
            EditorGUILayout.Separator();

            //warning on duplicate key
            if (((IKeyedItem)item.item).isDuplicate)
            {
                EditorGUILayout.HelpBox("Duplicate Key", MessageType.Error);
            }

            return result;
        }

        private void AddNew(Vector2 mousePos, AIInspectorState state)
        {
            if (_dictionary == null)
            {
                var dict = (IDictionary)Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(_keyType, _valueType));
                _setter(dict);
            }

            if (_itemCreator == null)
            {
                _itemCreator = new ItemConstructor(_dictionary, _keyType, _valueType);
            }

            var itemWrapper = _itemCreator.Conctruct();
            _editorItems.Add(ReflectMaster.Reflect(itemWrapper));

            // We do not want the button click itself to count as a change.. same as above.
            GUI.changed = false;
        }

        private class ItemConstructor
        {
            private static readonly Type _baseType = typeof(DictionaryItem<,>);

            private Type _itemType;
            private object[] _ctorParams;

            internal ItemConstructor(IDictionary parent, Type keyType, Type valueType)
            {
                _itemType = _baseType.MakeGenericType(keyType, valueType);
                _ctorParams = new object[3];
                _ctorParams[2] = parent;
            }

            internal void SetParent(IDictionary parent)
            {
                _ctorParams[2] = parent;
            }

            internal object Conctruct(object key, object value)
            {
                _ctorParams[0] = key;
                _ctorParams[1] = value;
                return Activator.CreateInstance(_itemType, _ctorParams);
            }

            internal object Conctruct()
            {
                return Activator.CreateInstance(_itemType, _ctorParams[2]);
            }
        }
    }
}