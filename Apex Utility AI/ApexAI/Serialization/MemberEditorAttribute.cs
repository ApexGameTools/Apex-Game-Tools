/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Serialization
{
    using System;

    /// <summary>
    /// Base class for attributes used to designate which renderer to use when displaying the member in the editor.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public abstract class MemberEditorAttribute : Attribute
    {
    }
}
