/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.LoadBalancing
{
    using System;
    using System.Diagnostics;
    using Apex.DataStructures;

    /// <summary>
    /// Utility for marshalling an action from a thread onto the main thread.
    /// </summary>
    public sealed class Marshaller : IMarshaller
    {
        private readonly Stopwatch _watch;
        private readonly SimpleQueue<Action> _queue;
        private readonly int _maxMillisecondsPerFrame;

        internal Marshaller(int maxMillisecondsPerFrame)
        {
            _maxMillisecondsPerFrame = maxMillisecondsPerFrame;
            _queue = new SimpleQueue<Action>(10);
            _watch = new Stopwatch();
        }

        /// <summary>
        /// Executes the action on the main thread.
        /// </summary>
        /// <param name="a">The action.</param>
        public void ExecuteOnMainThread(Action a)
        {
            lock (_queue)
            {
                _queue.Enqueue(a);
            }
        }

        internal void ProcessPending()
        {
            if (_queue.count == 0)
            {
                return;
            }

            _watch.Start();

            do
            {
                Action next;
                lock (_queue)
                {
                    next = _queue.Dequeue();
                }

                next();
            }
            while (_queue.count > 0 && _watch.ElapsedMilliseconds < _maxMillisecondsPerFrame);

            _watch.Reset();
        }
    }
}
