/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Services
{
    /// <summary>
    /// Interface for components that handle a certain type of message.
    /// </summary>
    /// <typeparam name="T">The type of message handled</typeparam>
    public interface IHandleMessage<T>
    {
        /// <summary>
        /// Handles the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        void Handle(T message);
    }
}
