/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Services
{
    using UnityEngine;

    /// <summary>
    /// Factory for using <see cref="AdvancedMessageBus"/> for messaging. Attach this to the same GameObject as the <see cref="GameServicesInitializerComponent"/> to override the default <see cref="BasicMessageBus"/>.
    /// </summary>
    [AddComponentMenu("Apex/Game World/Advanced MessageBus", 1027)]
    [ApexComponent("Game World")]
    public class AdvancedMessageBusFactoryComponent : MonoBehaviour, IMessageBusFactory
    {
        /// <summary>
        /// Creates the message bus.
        /// </summary>
        /// <returns>
        /// The message bus
        /// </returns>
        public IMessageBus CreateMessageBus()
        {
            return new AdvancedMessageBus();
        }
    }
}
