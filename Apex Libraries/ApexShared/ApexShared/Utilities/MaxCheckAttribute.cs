/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Utilities
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Property attribute to ensure a max value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class MaxCheckAttribute : PropertyAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaxCheckAttribute"/> class.
        /// </summary>
        /// <param name="max">The maximum.</param>
        public MaxCheckAttribute(float max)
        {
            this.max = max;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaxCheckAttribute"/> class.
        /// </summary>
        /// <param name="max">The maximum.</param>
        public MaxCheckAttribute(int max)
        {
            this.max = max;
        }

        /// <summary>
        /// Gets the maximum.
        /// </summary>
        /// <value>
        /// The maximum.
        /// </value>
        public float max { get; private set; }

        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        public string label { get; set; }

        /// <summary>
        /// Gets or sets the tooltip.
        /// </summary>
        public string tooltip { get; set; }
    }
}
