﻿namespace Apex.Common
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Marker for Apex Attribute fields
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class AttributePropertyAttribute : PropertyAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AttributePropertyAttribute"/> class.
        /// </summary>
        public AttributePropertyAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributePropertyAttribute"/> class.
        /// </summary>
        /// <param name="label">The label.</param>
        public AttributePropertyAttribute(string label)
        {
            this.label = label;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributePropertyAttribute"/> class.
        /// </summary>
        /// <param name="label">The label.</param>
        /// <param name="tooltip">The tooltip</param>
        public AttributePropertyAttribute(string label, string tooltip)
        {
            this.label = label;
            this.tooltip = tooltip;
        }

        /// <summary>
        /// Gets the label.
        /// </summary>
        /// <value>
        /// The label.
        /// </value>
        public string label
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the tooltip.
        /// </summary>
        /// <value>
        /// The tooltip.
        /// </value>
        public string tooltip
        {
            get;
            private set;
        }
    }
}
