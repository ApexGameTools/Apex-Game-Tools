/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Editor.Reflection
{
    using System.Collections;
    using Apex.Serialization;

    [Hidden, FriendlyName("")]
    internal sealed class SimpleListItem<T>
    {
        private T _value;
        private int _idx;
        private IList _parent;

        public SimpleListItem(T value, int idx, IList parent)
        {
            _value = value;
            _idx = idx;
            _parent = parent;
        }

        [ApexSerialization]
        internal T value
        {
            get
            {
                return _value;
            }

            set
            {
                _value = value;
                _parent[_idx] = value;
            }
        }
    }
}
