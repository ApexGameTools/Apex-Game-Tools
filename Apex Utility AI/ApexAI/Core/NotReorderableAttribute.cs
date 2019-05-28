/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI
{
    using System;

    /// <summary>
    /// Marks a list property or field as not reorderable in the UI.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class NotReorderableAttribute : Attribute
    {
    }
}