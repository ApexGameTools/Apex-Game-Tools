/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI
{
    using System;

    /// <summary>
    /// An attribute to decorate AI entity fields and properties to give them a friendly name, description and sort order in the Editor inspector UI.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class FriendlyNameAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FriendlyNameAttribute"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public FriendlyNameAttribute(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FriendlyNameAttribute"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        public FriendlyNameAttribute(string name, string description)
        {
            this.name = name;
            this.description = description;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string name
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string description
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the sort order.
        /// </summary>
        /// <value>
        /// The sort order.
        /// </value>
        public int sortOrder
        {
            get;
            set;
        }
    }
}
