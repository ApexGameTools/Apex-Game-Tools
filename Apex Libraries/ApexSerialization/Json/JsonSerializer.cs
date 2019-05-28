/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Serialization.Json
{
    using System;

    /// <summary>
    /// The default serializer. Serializes to and from json.
    /// </summary>
    /// <seealso cref="Apex.Serialization.ISerializer" />
    internal class JsonSerializer : ISerializer
    {
        /// <summary>
        /// Deserializes the serialized representation of an object to staging.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>
        /// The staged representation of the object.
        /// </returns>
        public StageItem Deserialize(string data)
        {
            var parser = new JsonParser();
            return parser.Parse(data);
        }

        /// <summary>
        /// Serializes the specified staged object (or object graph).
        /// </summary>
        /// <param name="item">The staged representation of the object to serialize.</param>
        /// <param name="pretty">Whether to serialize the data in a pretty (reader friendly) format.</param>
        /// <returns>
        /// The serialized representation of the object.
        /// </returns>
        /// <exception cref="System.ArgumentException">Only StageElements can serve as the root of a serialized graph.</exception>
        public string Serialize(StageItem item, bool pretty)
        {
            var root = item as StageElement;
            if (root == null)
            {
                throw new ArgumentException("Only StageElements can serve as the root of a serialized graph.");
            }

            var s = new StagedToJson(pretty);
            return s.Serialize(root);
        }
    }
}
