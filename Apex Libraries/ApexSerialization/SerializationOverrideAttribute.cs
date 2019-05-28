/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Serialization
{
    using System;

    /// <summary>
    /// Attribute used to decorate <see cref="ISerializer"/>s, <see cref="IStager"/>s and <see cref="IValueConverter"/>s,
    /// to have them override the default implementations.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SerializationOverrideAttribute : Attribute
    {
    }
}
