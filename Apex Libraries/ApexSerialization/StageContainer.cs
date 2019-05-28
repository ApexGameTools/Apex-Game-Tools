/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Serialization
{
    using System.Collections.Generic;

    /// <summary>
    /// Base class for staged items that represent an object graph.
    /// </summary>
    /// <seealso cref="Apex.Serialization.StageItem" />
    public abstract class StageContainer : StageItem
    {
        internal StageItem _tailChild;

        /// <summary>
        /// Initializes a new instance of the <see cref="StageContainer"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        protected StageContainer(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Adds the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        public abstract void Add(StageItem item);

        internal abstract void Remove(StageItem item);

        /// <summary>
        /// Gets all child items.
        /// </summary>
        /// <returns>All child items.</returns>
        public IEnumerable<StageItem> Items()
        {
            if (_tailChild == null)
            {
                yield break;
            }

            var current = _tailChild;

            do
            {
                current = current.next;
                yield return current;
            }
            while (current != _tailChild);
        }

        /// <summary>
        /// Gets all child <see cref="StageElement"/>s.
        /// </summary>
        /// <returns>All child elements.</returns>
        public IEnumerable<StageElement> Elements()
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
                if (el != null)
                {
                    yield return el;
                }
            }
            while (current != _tailChild);
        }

        /// <summary>
        /// Gets all descendant items.
        /// </summary>
        /// <returns>All descendant items.</returns>
        public IEnumerable<StageItem> Descendants()
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

                yield return current;

                curElement = current as StageContainer;
            }
        }

        /// <summary>
        /// Gets all descendants of a particular type.
        /// </summary>
        /// <typeparam name="T">The type of descendant.</typeparam>
        /// <returns>All descendants of the specified type.</returns>
        public IEnumerable<T> Descendants<T>() where T : StageItem
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

                var el = current as T;
                if (el != null)
                {
                    yield return el;
                }

                curElement = current as StageContainer;
            }
        }
    }
}
