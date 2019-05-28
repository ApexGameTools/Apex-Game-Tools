/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor.Reflection
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Apex.AI.Editor.UndoRedo;

    [TypesHandled(typeof(List<>))]
    public sealed class ListField : ListFieldBase
    {      
        public ListField(MemberData data, object owner)
            : base(data, owner, false)
        {
        }

        protected override Type GetItemType(Type listType)
        {
            return listType.GetGenericArguments()[0];
        }

        protected override void DoRemove(int removeIdx, AIInspectorState state)
        {
            var item = _list[removeIdx];
            _list.RemoveAt(removeIdx);
            state.currentAIUI.undoRedo.Do(new ListFieldRemoveOperation(_list, removeIdx, item));
        }

        protected override void DoAdd(object item, AIInspectorState state)
        {
            _list.Add(item);
            state.currentAIUI.undoRedo.Do(new ListFieldAddOperation(_list, item));
        }

        protected override IList CreateList()
        {
            return (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(_itemType));
        }
    }
}