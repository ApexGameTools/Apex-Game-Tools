/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Serialization
{
    using System;

    /// <summary>
    /// Staged representation of a list or array.
    /// </summary>
    /// <seealso cref="Apex.Serialization.StageContainer" />
    public sealed class StageList : StageContainer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StageList"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public StageList(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StageList"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="items">The items.</param>
        public StageList(string name, params StageItem[] items)
            : base(name)
        {
            if (items != null)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    Add(items[i]);
                }
            }
        }

        /// <summary>
        /// Adds the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        public override void Add(StageItem item)
        {
            if (item == null)
            {
                return;
            }

            if (_tailChild == null)
            {
                _tailChild = item;
                _tailChild.next = _tailChild;
            }
            else
            {
                item.next = _tailChild.next;
                _tailChild.next = item;
                _tailChild = item;
            }

            item.parent = this;
        }

        internal override void Remove(StageItem item)
        {
            if (item == null)
            {
                return;
            }

            if (item.parent != this)
            {
                throw new ArgumentException("Cannot remove item not belonging to this element.", "item");
            }

            if (item.next == item)
            {
                _tailChild = null;
            }
            else
            {
                var current = _tailChild;
                while (current.next != item)
                {
                    current = current.next;
                }

                current.next = item.next;

                if (item == _tailChild)
                {
                    _tailChild = current;
                }
            }

            item.next = null;
            item.parent = null;
        }
    }
}
