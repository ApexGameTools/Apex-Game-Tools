/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Serialization
{
    using System;

    /// <summary>
    /// Optional attribute to mark types that are serialized as part of an Apex serialization process.
    /// Some types may not expose any serialized fields or properties (<see cref="ApexSerializationAttribute"/>) in which case marking them with this will help identify them as serialized types.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class ApexSerializedTypeAttribute : Attribute
    {
    }
}
