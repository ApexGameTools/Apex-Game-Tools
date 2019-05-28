namespace Apex.Serialization
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Staged representation of a class or struct.
    /// </summary>
    /// <seealso cref="Apex.Serialization.StageContainer" />
    public sealed class StageElement : StageContainer
    {
        private StageItem _tailAttribute;

        /// <summary>
        /// Initializes a new instance of the <see cref="StageElement"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public StageElement(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StageElement"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="items">The child items.</param>
        public StageElement(string name, params StageItem[] items)
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
        /// Get all attributes of this element.
        /// </summary>
        /// <returns>All attributes</returns>
        public IEnumerable<StageAttribute> Attributes()
        {
            if (_tailAttribute == null)
            {
                yield break;
            }

            var current = _tailAttribute.next;

            while (current != _tailAttribute)
            {
                yield return (StageAttribute)current;
                current = current.next;
            }

            yield return (StageAttribute)current;
        }

        /// <summary>
        /// Gets all child items with the specified name.
        /// </summary>
        /// <returns>All child items with the specified name.</returns>
        public IEnumerable<StageItem> Items(string name)
        {
            if (_tailChild == null)
            {
                yield break;
            }

            var current = _tailChild;

            do
            {
                current = current.next;
                if (current.name == name)
                {
                    yield return current;
                }
            }
            while (current != _tailChild);
        }

        /// <summary>
        /// Gets all child elements with the specified name.
        /// </summary>
        /// <returns>All child elements with the specified name.</returns>
        public IEnumerable<StageElement> Elements(string name)
        {
            if (_tailChild == null)
            {
                yield break;
            }

            var current = _tailChild;

            do
            {
                current = current.next;
                var el = current as StageElement;
                if (el != null && el.name == name)
                {
                    yield return el;
                }
            }
            while (current != _tailChild);
        }

        /// <summary>
        /// Gets all descendant items with the specified name.
        /// </summary>
        /// <returns>All descendant items with the specified name.</returns>
        public IEnumerable<StageItem> Descendants(string name)
        {
            StageItem current = this;
            StageContainer curElement = this;

            while (true)
            {
                if (curElement == null || curElement._tailChild == null)
                {
                    while (current != this && current == current.parent._tailChild)
                    {
                        current = current.parent;
                    }

                    if (current == this)
                    {
                        break;
                    }

                    current = current.next;
                }
                else
                {
                    current = curElement._tailChild.next;
                }

                if (current.name == name)
                {
                    yield return current;
                }

                curElement = current as StageContainer;
            }
        }

        /// <summary>
        /// Gets the first item with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The first item with the specified name.</returns>
        public StageItem Item(string name)
        {
            if (_tailChild == null)
            {
                return null;
            }

            var current = _tailChild;
            do
            {
                if (current.name == name)
                {
                    return current;
                }

                current = current.next;
            }
            while (current != _tailChild);

            return null;
        }

        /// <summary>
        /// Gets the first element with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The first element with the specified name.</returns>
        public StageElement Element(string name)
        {
            return Item(name) as StageElement;
        }

        /// <summary>
        /// Gets the first list with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The first list with the specified name.</returns>
        public StageList List(string name)
        {
            return Item(name) as StageList;
        }

        /// <summary>
        /// Gets the first attribute with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The first attribute with the specified name.</returns>
        public StageAttribute Attribute(string name)
        {
            if (_tailAttribute == null)
            {
                return null;
            }

            var current = _tailAttribute;
            do
            {
                if (current.name == name)
                {
                    return (StageAttribute)current;
                }

                current = current.next;
            }
            while (current != _tailAttribute);

            return null;
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

            if (item is StageAttribute)
            {
                if (_tailAttribute == null)
                {
                    _tailAttribute = item;
                    _tailAttribute.next = _tailAttribute;
                }
                else
                {
                    item.next = _tailAttribute.next;
                    _tailAttribute.next = item;
                    _tailAttribute = item;
                }
            }
            else
            {
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
                if (item is StageAttribute)
                {
                    _tailAttribute = null;
                }
                else
                {
                    _tailChild = null;
                }
            }
            else
            {
                var isAttribute = (item is StageAttribute);
                var current = isAttribute ? _tailAttribute : _tailChild;
                while (current.next != item)
                {
                    current = current.next;
                }

                current.next = item.next;

                if (isAttribute && item == _tailAttribute)
                {
                    _tailAttribute = current;
                }
                else if (item == _tailChild)
                {
                    _tailChild = current;
                }
            }

            item.next = null;
            item.parent = null;
        }
    }
}
