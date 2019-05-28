/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Services
{
    /// <summary>
    /// Interface for message bus factories.
    /// Implement this on a MonoBehaviour class and attach it to the same GameObject as the <see cref="GameServicesInitializerComponent"/> to override the default path smoother.
    /// </summary>
    public interface IMessageBusFactory
    {
        /// <summary>
        /// Creates the message bus.
        /// </summary>
        /// <returns>The message bus</returns>
        IMessageBus CreateMessageBus();
    }
}
