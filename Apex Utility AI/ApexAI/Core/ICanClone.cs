/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI
{
    /// <summary>
    /// Interface for entities that can clone or transfer settings from another entity to itself.
    /// </summary>
    public interface ICanClone
    {
        /// <summary>
        /// Clones or transfers settings from the other entity to itself.
        /// </summary>
        /// <param name="other">The other entity.</param>
        void CloneFrom(object other);
    }
}
