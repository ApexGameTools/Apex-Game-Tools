/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Serialization
{
    /// <summary>
    /// Interface to implement on entities that need to initialize after deserialization.
    /// </summary>
    public interface IInitializeAfterDeserialization
    {
        /// <summary>
        /// Will be called by the engine after deserialization is complete, allowing the implementing entity to initialize itself.
        /// </summary>
        /// <param name="rootObject">The root object of the deserialization process.</param>
        void Initialize(object rootObject);
    }
}
