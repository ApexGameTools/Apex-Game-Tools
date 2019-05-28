/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI
{
    using System;

    /// <summary>
    /// Attribute used to categorize AI entities.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class AICategoryAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AICategoryAttribute"/> class.
        /// </summary>
        /// <param name="name">The category name.</param>
        public AICategoryAttribute(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AICategoryAttribute"/> class.
        /// </summary>
        /// <param name="name">The category name.</param>
        /// <param name="sortOrder">The sort order.</param>
        public AICategoryAttribute(string name, int sortOrder)
        {
            this.name = name;
            this.sortOrder = sortOrder;
        }

        /// <summary>
        /// Gets the category name.
        /// </summary>
        /// <value>
        /// The category name.
        /// </value>
        public string name
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the sort order.
        /// </summary>
        /// <value>
        /// The sort order.
        /// </value>
        public int sortOrder
        {
            get;
            private set;
        }
    }
}
