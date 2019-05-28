/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Editor.Reflection
{
    using System;
    using System.Collections;
    using Apex.AI.Editor.UndoRedo;

    [TypesHandled(typeof(Array))]
    public sealed class ArrayField : ListFieldBase
    {      
        public ArrayField(MemberData data, object owner)
            : base(data, owner, true)
        {
        }

        protected override Type GetItemType(Type listType)
        {
            return listType.GetElementType();
        }

        protected override void DoRemove(int removeIdx, AIInspectorState state)
        {
            var item = _list[removeIdx];

            var arr = (Array)_list;
            var tmp = Array.CreateInstance(_itemType, arr.Length - 1);
            Array.Copy(arr, 0, tmp, 0, removeIdx);
            if (removeIdx < tmp.Length)
            {
                Array.Copy(arr, removeIdx + 1, tmp, removeIdx, tmp.Length - removeIdx);
            }

            _setter(tmp);

            state.currentAIUI.undoRedo.Do(new ArrayFieldRemoveOperation(tmp, _setter, removeIdx, item));
        }

        protected override void DoAdd(object item, AIInspectorState state)
        {
            var arr = (Array)_list;
            var tmp = Array.CreateInstance(_itemType, arr.Length + 1);
            Array.Copy(arr, 0, tmp, 0, arr.Length);
            tmp.SetValue(item, arr.Length);
            _setter(tmp);

            state.currentAIUI.undoRedo.Do(new ArrayFieldAddOperation(tmp, _setter, item));
        }

        protected override IList CreateList()
        {
            return Array.CreateInstance(_itemType, 0);
        }
    }
}