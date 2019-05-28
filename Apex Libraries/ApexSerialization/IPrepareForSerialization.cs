/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Serialization
{
    /// <summary>
    /// Interface to implement on entities that need to prepare before serialization.
    /// </summary>
    public interface IPrepareForSerialization
    {
        /// <summary>
        ///  Will be called by the engine just before the entity is serialized.
        /// </summary>
        void Prepare();
    }
}
