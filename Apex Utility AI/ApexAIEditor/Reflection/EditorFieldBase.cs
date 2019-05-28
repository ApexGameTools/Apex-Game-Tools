/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor.Reflection
{
    using System;
    using System.Reflection;
    using Apex.AI.Editor.UndoRedo;
    using UnityEngine;

    public abstract class EditorFieldBase<T> : IEditorField
    {
        protected T _curValue;
        protected GUIContent _label;
        private object _owner;
        private MemberInfo _member;

        protected EditorFieldBase(MemberData data, object owner)
        {
            _label = new GUIContent(data.name, data.description);
            _member = data.member;
            _owner = owner;

            var prop = _member as PropertyInfo;
            if (prop != null)
            {
                _curValue = (T)prop.GetValue(_owner, null);
            }
            else
            {
                var field = _member as FieldInfo;
                if (field != null)
                {
                    _curValue = (T)field.GetValue(_owner);
                }
                else
                {
                    throw new ArgumentException("Invalid reflected member type, only fields and properties are supported.");
                }
            }
        }

        public string memberName
        {
            get { return _member.Name; }
        }

        public object currentValue
        {
            get { return _curValue; }
        }

        protected void UpdateValue(T newValue, AIInspectorState state)
        {
            state.currentAIUI.undoRedo.Do(new EditorFieldOperation<T>(_owner, _member, _curValue, newValue));
            state.MarkDirty();

            _curValue = newValue;

            var prop = _member as PropertyInfo;
            if (prop != null)
            {
                prop.SetValue(_owner, newValue, null);
            }
            else
            {
                var field = _member as FieldInfo;
                if (field != null)
                {
                    field.SetValue(_owner, newValue);
                }
            }
        }

        public abstract void RenderField(AIInspectorState state);
    }
}