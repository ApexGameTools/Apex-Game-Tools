/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex
{
    using System;

    /// <summary>
    /// Marks an Apex Component for identification by the <see cref="ApexComponentMaster"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class ApexComponentAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApexComponentAttribute"/> class.
        /// </summary>
        /// <param name="category">The category.</param>
        public ApexComponentAttribute(string category)
        {
            this.category = category;
        }

        /// <summary>
        /// Gets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        public string category
        {
            get;
            private set;
        }
    }
}
