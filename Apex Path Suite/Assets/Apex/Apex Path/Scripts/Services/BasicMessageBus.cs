/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Services
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Apex.LoadBalancing;

    /// <summary>
    /// This message bus is simple, fast and memory efficient. However it is also NOT thread safe and NOT leak proof.
    /// Subscribers MUST unsubscribe in order for them to be eligible for garbage collection.
    /// Seeing as thread safety is a non-issue with Unity and the fact that Unity provides a controlled life cycle, this message bus is optimal when message exchanges happen only between Unity derivatives.
    /// </summary>
    public class BasicMessageBus : IMessageBus
    {
        private IDictionary<Type, IList<object>> _subscriptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicMessageBus"/> class.
        /// </summary>
        public BasicMessageBus()
        {
            _subscriptions = new Dictionary<Type, IList<object>>();
        }

        /// <summary>
        /// Subscribes the specified subscriber.
        /// </summary>
        /// <typeparam name="T">The type of message being subscribed to</typeparam>
        /// <param name="subscriber">The subscriber.</param>
        public void Subscribe<T>(IHandleMessage<T> subscriber)
        {
            IList<object> subscribers;
            if (!_subscriptions.TryGetValue(typeof(T), out subscribers))
            {
                subscribers = new List<object>();
                _subscriptions.Add(typeof(T), subscribers);
            }

            subscribers.Add(subscriber);
        }

        /// <summary>
        /// Unsubscribes the specified subscriber.
        /// </summary>
        /// <typeparam name="T">The type of message being unsubscribed from</typeparam>
        /// <param name="subscriber">The subscriber.</param>
        public void Unsubscribe<T>(IHandleMessage<T> subscriber)
        {
            IList<object> subscribers;
            if (!_subscriptions.TryGetValue(typeof(T), out subscribers))
            {
                return;
            }

            subscribers.Remove(subscriber);

            if (subscribers.Count == 0)
            {
                _subscriptions.Remove(typeof(T));
            }
        }

        /// <summary>
        /// Posts the specified message.
        /// </summary>
        /// <typeparam name="T">The type of message</typeparam>
        /// <param name="message">The message.</param>
        public void Post<T>(T message)
        {
            IList<object> subscribers;
            if (!_subscriptions.TryGetValue(typeof(T), out subscribers))
            {
                return;
            }

            for (int i = subscribers.Count - 1; i >= 0; i--)
            {
                var subscriber = subscribers[i] as IHandleMessage<T>;
                subscriber.Handle(message);
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
            IList<object> subscribers;
            if (!_subscriptions.TryGetValue(typeof(T), out subscribers))
            {
                if (callback != null)
                {
                    callback();
                }

                return;
            }

            var task = new LongRunningAction(
                () => BalancedPoster(subscribers, message),
                maxMillisecondUsedPerFrame,
                callback);

            LoadBalancer.defaultBalancer.Add(task, 0f);
        }

        private IEnumerator BalancedPoster<T>(IList<object> subscribers, T message)
        {
            for (int i = subscribers.Count - 1; i >= 0; i--)
            {
                var subscriber = subscribers[i] as IHandleMessage<T>;
                subscriber.Handle(message);
                yield return null;
            }
        }
    }
}
