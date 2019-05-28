/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor.Reflection
{
    using System;
    using System.Reflection;
    using Apex.AI.Editor.UndoRedo;
    using Apex.Editor;
    using UnityEditor;
    using UnityEngine;

    public sealed class CustomField : IEditorField
    {
        private object _item;
        private Type _itemType;
        private EditorItem _editorItem;
        private Action<object> _setter;
        private GUIContent _label;

        public CustomField(MemberData data, object owner)
        {
            _label = new GUIContent(data.name, data.description);
            this.memberName = data.member.Name;

            var prop = data.member as PropertyInfo;
            if (prop != null)
            {
                _itemType = prop.PropertyType;
                _item = prop.GetValue(owner, null);
                _setter = (item) => prop.SetValue(owner, item, null);
            }
            else
            {
                var field = data.member as FieldInfo;
                if (field != null)
                {
                    _itemType = field.FieldType;
                    _item = field.GetValue(owner);
                    _setter = (item) => field.SetValue(owner, item);
                }
                else
                {
                    throw new ArgumentException("Invalid reflected member type, only fields and properties are supported.");
                }
            }

            if (_item != null)
            {
                _editorItem = ReflectMaster.Reflect(_item);
            }
        }

        public string memberName
        {
            get;
            private set;
        }

        public object currentValue
        {
            get { return _item; }
        }

        public void RenderField(AIInspectorState state)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(_label);

            if (GUILayout.Button(SharedStyles.changeSelectionTooltip, SharedStyles.BuiltIn.changeButtonSmall))
            {
                Action<Type> cb = (selectedType) =>
                {
                    if (_itemType.IsGenericType && selectedType.IsGenericType)
                    {
                        var genArgs = _itemType.GetGenericArguments();
                        selectedType = selectedType.GetGenericTypeDefinition().MakeGenericType(genArgs);
                    }

                    var old = _item;
                    _item = Activator.CreateInstance(selectedType);
                    _editorItem = ReflectMaster.Reflect(_item);
                    _setter(_item);

                    state.currentAIUI.undoRedo.Do(new CustomEditorFieldOperation(old, _item, _setter));
                    state.MarkDirty();
                };

                //We do not want the button click itself to count as a change.. same as above.
                GUI.changed = false;

                var screenPos = EditorGUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                AIEntitySelectorWindow.Get(screenPos, _itemType, cb);
            }

            EditorGUILayout.EndHorizontal();

            bool doDelete = false;
            if (_item != null)
            {
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.BeginHorizontal(SharedStyles.BuiltIn.listItemHeader);
                EditorGUILayout.LabelField(_editorItem.name, SharedStyles.BuiltIn.normalText);
                if (GUILayout.Button(SharedStyles.deleteTooltip, SharedStyles.BuiltIn.deleteButtonSmall))
                {
                    GUI.changed = false;

                    if (DisplayHelper.ConfirmDelete("item"))
                    {
                        doDelete = true;
                    }
                }

                EditorGUILayout.EndHorizontal();

                _editorItem.Render(state);
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Separator();

            //We do the delete outside any layout stuff to ensure we don't get weird warnings.
            if (doDelete)
            {
                state.currentAIUI.undoRedo.Do(new CustomEditorFieldOperation(_item, null, _setter));
                _setter(null);
                _item = null;
                _editorItem = null;
                state.MarkDirty();
            }
        }
    }
}