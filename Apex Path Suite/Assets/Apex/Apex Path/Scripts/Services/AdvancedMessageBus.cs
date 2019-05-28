/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Services
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Apex.LoadBalancing;

    /// <summary>
    /// This message bus is thread safe and leak proof. However this also means that it uses more memory and is less efficient than its simple counterpart <see cref="BasicMessageBus"/>.
    /// Use this only if you spin up threads on your own and/or need the message bus to work safely with subscribers that do not have an easily controlled life cycle.
    /// </summary>
    public class AdvancedMessageBus : IMessageBus
    {
        private IDictionary<Type, IList<WeakReference>> _subscriptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdvancedMessageBus"/> class.
        /// </summary>
        public AdvancedMessageBus()
        {
            _subscriptions = new Dictionary<Type, IList<WeakReference>>();
        }

        /// <summary>
        /// Subscribes the specified subscriber.
        /// </summary>
        /// <typeparam name="T">The type of message being subscribed to</typeparam>
        /// <param name="subscriber">The subscriber.</param>
        public void Subscribe<T>(IHandleMessage<T> subscriber)
        {
            IList<WeakReference> subscribers;

            lock (_subscriptions)
            {
                if (!_subscriptions.TryGetValue(typeof(T), out subscribers))
                {
                    subscribers = new List<WeakReference>();
                    _subscriptions.Add(typeof(T), subscribers);
                }

                lock (subscribers)
                {
                    subscribers.Add(new WeakReference(subscriber));
                }
            }
        }

        /// <summary>
        /// Unsubscribes the specified subscriber.
        /// </summary>
        /// <typeparam name="T">The type of message being unsubscribed from</typeparam>
        /// <param name="subscriber">The subscriber.</param>
        public void Unsubscribe<T>(IHandleMessage<T> subscriber)
        {
            IList<WeakReference> subscribers;

            lock (_subscriptions)
            {
                if (!_subscriptions.TryGetValue(typeof(T), out subscribers))
                {
                    return;
                }

                lock (subscribers)
                {
                    for (int i = 0; i < subscribers.Count; i++)
                    {
                        if (object.ReferenceEquals(subscribers[i].Target, subscriber))
                        {
                            subscribers.RemoveAt(i);
                            break;
                        }
                    }

                    if (subscribers.Count == 0)
                    {
                        _subscriptions.Remove(typeof(T));
                    }
                }
            }
        }

        /// <summary>
        /// Posts the specified message.
        /// </summary>
        /// <typeparam name="T">The type of message</typeparam>
        /// <param name="message">The message.</param>
        public void Post<T>(T message)
        {
            IList<WeakReference> subscribers;

            lock (_subscriptions)
            {
                if (!_subscriptions.TryGetValue(typeof(T), out subscribers))
                {
                    return;
                }
            }

            lock (subscribers)
            {
                for (int i = subscribers.Count - 1; i >= 0; i--)
                {
                    var subscriber = subscribers[i].Target as IHandleMessage<T>;

                    if (subscriber != null)
                    {
                        subscriber.Handle(message);
                    }
                    else
                    {
                        subscribers.RemoveAt(i);
                    }
                }
            }

            //Clean up subscriber list. Its done this rather cumbersome way since we want to separate the locks above in order not to lock the entire bus while posting a specific message, since message posting may take time.
            if (subscribers.Count == 0)
            {
                lock (_subscriptions)
                {
                    if (!_subscriptions.TryGetValue(typeof(T), out subscribers))
                    {
                        return;
                    }

                    lock (subscribers)
                    {
                        if (subscribers.Count == 0)
                        {
                            _subscriptions.Remove(typeof(T));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Return the number of subscribers for a specific message type.
        /// </summary>
        /// <typeparam name="T">The type of message</typeparam>
        /// <returns>The number of subscribers</returns>
        public int SubscribersFor<T>()
        {
            IList<WeakReference> subscribers;

            lock (_subscriptions)
            {
                if (!_subscriptions.TryGetValue(typeof(T), out subscribers))
                {
                    return 0;
                }

                lock (subscribers)
                {
                    return subscribers.Count;
                }
            }
        }

        /// <summary>
        /// Posts the message as a <see cref="Apex.LoadBalancing.LongRunningAction" />. Use this if you experience message processing to affect the frame rate.
        /// </summary>
        /// <typeparam name="T">The type of message</typeparam>
        /// <param name="message">The message.</param>
        /// <param name="maxMillisecondUsedPerFrame">The maximum milliseconds used per frame for subscribers processing the message.</param>
        public void PostBalanced<T>(T message, int maxMillisecondUsedPerFrame)
        {
            PostBalanced(message, maxMillisecondUsedPerFrame, null);
        }

        /// <summary>
        /// Posts the message as a <see cref="Apex.LoadBalancing.LongRunningAction" />. Use this if you experience message processing to affect the frame rate.
        /// </summary>
        /// <typeparam name="T">The type of message</typeparam>
        /// <param name="message">The message.</param>
        /// <param name="maxMillisecondUsedPerFrame">The maximum milliseconds used per frame for subscribers processing the message.</param>
        /// <param name="callback">A callback which will be invoked once the message has been sent and processed by all subscribers.</param>
        public void PostBalanced<T>(T message, int maxMillisecondUsedPerFrame, Action callback)
        {
            IList<WeakReference> subscribers;

            lock (_subscriptions)
            {
                if (!_subscriptions.TryGetValue(typeof(T), out subscribers))
                {
                    if (callback != null)
                    {
                        callback();
                    }

                    return;
                }
            }

            var task = new LongRunningAction(
                () => BalancedPoster(subscribers, message),
                maxMillisecondUsedPerFrame,
                callback);

            LoadBalancer.defaultBalancer.Add(task, 0f);
        }

        private IEnumerator BalancedPoster<T>(IList<WeakReference> subscribers, T message)
        {
            lock (subscribers)
            {
                for (int i = subscribers.Count - 1; i >= 0; i--)
                {
                    var subscriber = subscribers[i].Target as IHandleMessage<T>;

                    if (subscriber != null)
                    {
                        subscriber.Handle(message);
                    }
                    else
                    {
                        subscribers.RemoveAt(i);
                    }

                    yield return null;
                }
            }

            //Clean up subscriber list. Its done this rather cumbersome way since we want to separate the locks above in order not to lock the entire bus while posting a specific message, since message posting may take time.
            if (subscribers.Count == 0)
            {
                lock (_subscriptions)
                {
                    if (!_subscriptions.TryGetValue(typeof(T), out subscribers))
                    {
                        yield break;
                    }

                    lock (subscribers)
                    {
                        if (subscribers.Count == 0)
                        {
                            _subscriptions.Remove(typeof(T));
                        }
                    }
                }
            }
        }
    }
}
