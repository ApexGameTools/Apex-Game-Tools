/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Messages
{
    using UnityEngine;

    /// <summary>
    /// A message for use with the <see cref="Apex.Services.IMessageBus"/> to signal unit death.
    /// </summary>
    public class UnitDeathMessage
    {
        /// <summary>
        /// Gets or sets the unit that died
        /// </summary>
        /// <value>
        /// The unit.
        /// </value>
        public Component unit { get; set; }
    }
}
