/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Utilities
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Property attribute to assign a label to a field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class LabelAttribute : PropertyAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LabelAttribute"/> class.
        /// </summary>
        /// <param name="label">The label.</param>
        public LabelAttribute(string label)
        {
            this.label = label;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LabelAttribute"/> class.
        /// </summary>
        /// <param name="label">The label.</param>
        /// <param name="tooltip">The tooltip</param>
        public LabelAttribute(string label, string tooltip)
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
        public string label { get; private set; }

        /// <summary>
        /// Gets the tooltip.
        /// </summary>
        /// <value>
        /// The tooltip.
        /// </value>
        public string tooltip { get; private set; }
    }
}
