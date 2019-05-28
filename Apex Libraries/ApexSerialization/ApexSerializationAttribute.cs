/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Serialization
{
    using System;

    /// <summary>
    /// Attribute to mark members (properties and fields) on entities that should be serialized as part of an Apex serialization process.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class ApexSerializationAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets a value indicating whether to hide this member in the editor.
        /// </summary>
        /// <value>
        ///   <c>true</c> to hide in editor; otherwise, <c>false</c>.
        /// </value>
        public bool hideInEditor
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the default value. Members whose actual value matches the default value are not serialized (to save space).
        /// </summary>
        /// <value>
        /// The default value.
        /// </value>
        public object defaultValue
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value which allows the field or property to be excluded on a per request basis.
        /// </summary>
        /// <value>
        ///   <c>true</c> if optional; otherwise, <c>false</c>.
        /// </value>
        public int excludeMask
        {
            get;
            set;
        }
    }
}
