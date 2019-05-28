/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Services
{
    using System;

    /// <summary>
    /// Interface for message buses
    /// </summary>
    public interface IMessageBus
    {
        /// <summary>
        /// Subscribes the specified subscriber.
        /// </summary>
        /// <typeparam name="T">The type of message being subscribed to</typeparam>
        /// <param name="subscriber">The subscriber.</param>
        void Subscribe<T>(IHandleMessage<T> subscriber);

        /// <summary>
        /// Unsubscribes the specified subscriber.
        /// </summary>
        /// <typeparam name="T">The type of message being unsubscribed from</typeparam>
        /// <param name="subscriber">The subscriber.</param>
        void Unsubscribe<T>(IHandleMessage<T> subscriber);

        /// <summary>
        /// Posts the specified message.
        /// </summary>
        /// <typeparam name="T">The type of message</typeparam>
        /// <param name="message">The message.</param>
        void Post<T>(T message);

        /// <summary>
        /// Posts the message as a <see cref="Apex.LoadBalancing.LongRunningAction"/>. Use this if you experience message processing to affect the frame rate.
        /// </summary>
        /// <typeparam name="T">The type of message</typeparam>
        /// <param name="message">The message.</param>
        /// <param name="maxMillisecondUsedPerFrame">The maximum milliseconds used per frame for subscribers processing the message.</param>
        void PostBalanced<T>(T message, int maxMillisecondUsedPerFrame);

        /// <summary>
        /// Posts the message as a <see cref="Apex.LoadBalancing.LongRunningAction"/>. Use this if you experience message processing to affect the frame rate.
        /// </summary>
        /// <typeparam name="T">The type of message</typeparam>
        /// <param name="message">The message.</param>
        /// <param name="maxMillisecondUsedPerFrame">The maximum milliseconds used per frame for subscribers processing the message.</param>
        /// <param name="callback">A callback which will be invoked once the message has been sent and processed by all subscribers.</param>
        void PostBalanced<T>(T message, int maxMillisecondUsedPerFrame, Action callback);
    }
}
