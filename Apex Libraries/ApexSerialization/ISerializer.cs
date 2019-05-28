/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Serialization
{
    /// <summary>
    /// Interface for types responsible for serialization.
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        /// Serializes the specified staged object (or object graph).
        /// </summary>
        /// <param name="item">The staged representation of the object to serialize.</param>
        /// <param name="pretty">Whether to serialize the data in a pretty (reader friendly) format.</param>
        /// <returns>The serialized representation of the object.</returns>
        string Serialize(StageItem item, bool pretty);

        /// <summary>
        /// Deserializes the serialized representation of an object to staging.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>The staged representation of the object.</returns>
        StageItem Deserialize(string data);
    }
}
