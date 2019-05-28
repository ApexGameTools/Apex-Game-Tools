/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.PathFinding
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Threading;
    using Apex.DataStructures;
    using Apex.Utilities;
    using Apex.WorldGeometry;
#if NETFX_CORE
    using Windows.System.Threading;
#endif

    /// <summary>
    /// The path service. This service will satisfy <see cref="IPathRequest"/>s
    /// </summary>
    public sealed class PathService : IPathService
    {
        private const int StartQueueSize = 10;

        private Stopwatch _stopwatch;
        private PriorityQueueFifo<IPathRequest> _queue;
        private IPathingEngine _engine;
        private bool _threadPoolSupported = true;
        private bool _processingActive;

#if !NETFX_CORE
        private AutoResetEvent _waitHandle;
        private IThreadFactory _threadFactory;
        private Thread _dedicatedThread;
#endif
        /// <summary>
        /// Initializes a new instance of the <see cref="PathService"/> class.
        /// </summary>
        /// <param name="engine">The engine.</param>
        /// <param name="threadFactory">The thread factory.</param>
        public PathService(IPathingEngine engine, IThreadFactory threadFactory)
            : this(engine, threadFactory, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PathService"/> class.
        /// </summary>
        /// <param name="engine">The engine.</param>
        /// <param name="threadFactory">The thread factory.</param>
        /// <param name="useThreadPoolForAsync">if set to <c>true</c> the thread pool will be used for async operation.</param>
        public PathService(IPathingEngine engine, IThreadFactory threadFactory, bool useThreadPoolForAsync)
        {
            Ensure.ArgumentNotNull(engine, "engine");
            Ensure.ArgumentNotNull(threadFactory, "threadFactory");

            _stopwatch = new Stopwatch();
            _queue = new PriorityQueueFifo<IPathRequest>(StartQueueSize, QueueType.Max);
            _engine = engine;

            _threadPoolSupported = useThreadPoolForAsync;

#if !NETFX_CORE
            _threadFactory = threadFactory;
#endif
        }

        /// <summary>
        /// Occurs if asynchronous processing failed.
        /// </summary>
        public Action OnAsyncFailed
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to run asynchronously.
        /// </summary>
        /// <value>
        ///   <c>true</c> if to run asynchronously; otherwise, <c>false</c>.
        /// </value>
        public bool runAsync
        {
            get;
            set;
        }

        /// <summary>
        /// Queues a request.
        /// </summary>
        /// <param name="request">The request.</param>
        public void QueueRequest(IPathRequest request)
        {
            QueueRequest(request, 0);
        }

        /// <summary>
        /// Queues a request with a priority. Higher values take priority.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="priority">The priority.</param>
        public void QueueRequest(IPathRequest request, int priority)
        {
            Ensure.ArgumentNotNull(request, "request");

            lock (_queue)
            {
                _queue.Enqueue(request, priority);

                if (this.runAsync && !_processingActive)
                {
                    StartAsyncProcessing();
                }
            }
        }

        /// <summary>
        /// Processes queued requests as a coroutine.
        /// </summary>
        /// <param name="maxMillisecondsPerFrame">The maximum milliseconds per frame before yielding.</param>
        /// <returns>
        /// coroutine enumerator
        /// </returns>
        /// <exception cref="System.InvalidOperationException">Cannot process as coroutine when set to async operation.</exception>
        public IEnumerator ProcessRequests(int maxMillisecondsPerFrame)
        {
            if (this.runAsync)
            {
                throw new InvalidOperationException("Cannot process as coroutine when set to async operation.");
            }

            while (!this.runAsync)
            {
                var next = GetNext();
                if (next == null)
                {
                    if (_stopwatch.IsRunning)
                    {
                        _stopwatch.Reset();
                    }

                    yield return null;
                }
                else
                {
                    var run = true;
                    var subIter = _engine.ProcessRequestCoroutine(next);

                    while (run)
                    {
                        //Start is called multiple places, due to the enumeration going on in various loops. Start is safe to call multiple times, it will simply do nothing if already started.
                        _stopwatch.Start();
                        run = subIter.MoveNext();

                        if (_stopwatch.ElapsedMilliseconds > maxMillisecondsPerFrame)
                        {
                            _stopwatch.Reset();
                            yield return null;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Processes queued requests synchronously.
        /// </summary>
        public void ProcessRequests()
        {
            var next = GetNext();
            while (next != null)
            {
                _engine.ProcessRequest(next);
                next = GetNext();
            }
        }

        private void ProcessRequestsAsync(object ignored)
        {
            try
            {
                ProcessRequests();
            }
            catch (OutOfMemoryException)
            {
                throw;
            }
            catch
            {
                _processingActive = false;
            }
        }

#if !NETFX_CORE
        private void ProcessRequestsAsyncDedicated(object ignored)
        {
            try
            {
                while (this.runAsync)
                {
                    ProcessRequests();
                    _waitHandle.WaitOne();
                }
            }
            catch (OutOfMemoryException)
            {
                throw;
            }
            catch
            {
                //Since this isn't exactly something we expect to happen, just null the thread ref so that the next time a request is made the thread is tarted again.
                _processingActive = false;
                _dedicatedThread = null;
            }
        }
#endif
        private void StartAsyncProcessing()
        {
            _processingActive = true;
#if NETFX_CORE
            if (_threadPoolSupported)
            {
                try
                {
                    ThreadPool.RunAsync(ProcessRequestsAsync);
                }
                catch (NotSupportedException)
                {
                    _threadPoolSupported = false;
                }
            }

            if (!_threadPoolSupported)
            {
                var e = OnAsyncFailed;
                if (e != null)
                {
                    e();
                }
            }
#else
            if (_threadPoolSupported)
            {
                try
                {
                    _threadPoolSupported = ThreadPool.QueueUserWorkItem(ProcessRequestsAsync);
                }
                catch (NotSupportedException)
                {
                    _threadPoolSupported = false;
                }
            }

            if (!_threadPoolSupported)
            {
                if (_dedicatedThread != null)
                {
                    _waitHandle.Set();
                    return;
                }

                try
                {
                    if (_waitHandle == null)
                    {
                        _waitHandle = new AutoResetEvent(true);
                    }

                    var t = _threadFactory.CreateThread("Pathing", ProcessRequestsAsyncDedicated);
                    t.IsBackground = true;

                    _dedicatedThread = t;
                    t.Start();
                }
                catch
                {
                    this.runAsync = false;

                    if (_waitHandle != null)
                    {
                        _waitHandle.Close();
                    }

                    _processingActive = false;
                    _dedicatedThread = null;
                    _waitHandle = null;

                    var e = OnAsyncFailed;
                    if (e != null)
                    {
                        e();
                    }
                }
            }
#endif
        }

        private IPathRequest GetNext()
        {
            lock (_queue)
            {
                while (_queue.hasNext)
                {
                    var next = _queue.Dequeue();

                    if (!next.hasDecayed)
                    {
                        return next;
                    }

                    next.Complete(new PathResult(PathingStatus.Decayed, null, 0, next));
                }

                _processingActive = false;
            }

            return null;
        }

        /// <summary>
        /// Releases handles. The path service is void after being disposed.
        /// </summary>
        public void Dispose()
        {
            lock (_queue)
            {
                _queue.Clear();
                this.runAsync = false;
            }

#if !NETFX_CORE
            if (_waitHandle != null)
            {
                _waitHandle.Set();
                _waitHandle.Close();
            }

            _dedicatedThread = null;
            _waitHandle = null;
#endif
        }
    }
}
