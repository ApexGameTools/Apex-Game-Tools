/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Utilities
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Property attribute to ensure a min value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class MinCheckAttribute : PropertyAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MinCheckAttribute"/> class.
        /// </summary>
        /// <param name="min">The minimum.</param>
        public MinCheckAttribute(float min)
        {
            this.min = min;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MinCheckAttribute"/> class.
        /// </summary>
        /// <param name="min">The minimum.</param>
        public MinCheckAttribute(int min)
        {
            this.min = min;
        }

        /// <summary>
        /// Gets the minimum.
        /// </summary>
        /// <value>
        /// The minimum.
        /// </value>
        public float min { get; private set; }

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
