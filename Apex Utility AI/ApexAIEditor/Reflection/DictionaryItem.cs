/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Editor.Reflection
{
    using System.Collections;
    using Apex.Serialization;

    [Hidden, FriendlyName("")]
    internal sealed class DictionaryItem<TKey, TValue> : IKeyedItem
    {
        private TKey _key;
        private TValue _value;
        private IDictionary _parent;

        public DictionaryItem(IDictionary parent)
        {
            _parent = parent;
        }

        public DictionaryItem(TKey key, TValue value, IDictionary parent)
        {
            _value = value;
            _key = key;
            _parent = parent;
        }

        [ApexSerialization]
        internal TKey key
        {
            get
            {
                return _key;
            }

            set
            {
                this.isDuplicate = _parent.Contains(value) && !_key.Equals(value);
                if (this.isDuplicate)
                {
                    return;
                }

                if (_key != null)
                {
                    _parent.Remove(_key);
                }

                _key = value;
                _parent[_key] = this.value;
            }
        }

        [ApexSerialization]
        internal TValue value
        {
            get
            {
                return _value;
            }

            set
            {
                _value = value;

                if (_key != null)
                {
                    _parent[_key] = value;
                }
            }
        }

        public bool isDuplicate
        {
            get;
            private set;
        }

        object IKeyedItem.key
        {
            get { return this.key; }
        }
    }
}
