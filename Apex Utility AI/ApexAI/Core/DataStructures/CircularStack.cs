/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.DataStructures
{
    using System;

    /// <summary>
    /// A circular stack ADT, i.e. fixed size.
    /// </summary>
    /// <typeparam name="T">The item type</typeparam>
    public class CircularStack<T>
    {
        private T[] _array;
        private int _used;
        private int _head = -1;

        /// <summary>
        /// Initializes a new instance of the <see cref="CircularStack{T}"/> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        public CircularStack(int capacity)
        {
            _array = new T[capacity];
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int count
        {
            get { return _used; }
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            //Just clear the lot. Yes its not the most optimized, but...
            Array.Clear(_array, 0, _array.Length);
            _used = 0;
            _head = -1;
        }

        /// <summary>
        /// Returns the item at the top of the stack, but does not remove it.
        /// </summary>
        /// <returns>The item at the top of the stack.</returns>
        /// <exception cref="System.InvalidOperationException">The stack is empty.</exception>
        public T Peek()
        {
            if (_used == 0)
            {
                throw new InvalidOperationException("The stack is empty.");
            }

            return _array[_head];
        }

        /// <summary>
        /// Removes and returns the item at the top of the stack.
        /// </summary>
        /// <returns>The item at the top of the stack</returns>
        /// <exception cref="System.InvalidOperationException">The stack is empty.</exception>
        public T Pop()
        {
            if (_used == 0)
            {
                throw new InvalidOperationException("The stack is empty.");
            }

            T t = _array[_head];
            _array[_head] = default(T);

            _used--;
            _head--;
            if (_head < 0 && _used > 0)
            {
                _head = _array.Length - 1;
            }

            return t;
        }

        /// <summary>
        /// Pushes the specified item onto the stack.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Push(T item)
        {
            _head = (_head + 1) % _array.Length;
            _array[_head] = item;

            if (_used < _array.Length)
            {
                _used++;
            }
        }
    }
}
