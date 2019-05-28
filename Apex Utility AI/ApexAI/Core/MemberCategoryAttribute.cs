/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI
{
    using System;

    /// <summary>
    ///  An attribute to decorate AI entity fields and properties to assign them to a category when displayed in the AI editor inspector.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class MemberCategoryAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MemberCategoryAttribute"/> class.
        /// </summary>
        /// <param name="name">The category name.</param>
        public MemberCategoryAttribute(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberCategoryAttribute"/> class.
        /// </summary>
        /// <param name="name">The category name.</param>
        /// <param name="sortOrder">The sort order.</param>
        public MemberCategoryAttribute(string name, int sortOrder)
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
