/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Utilities
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Property attribute to control the value range of a property and also allow setting a label and tooltip.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class RangeXAttribute : PropertyAttribute
    {
        /// <summary>
        /// The minimum
        /// </summary>
        public readonly float min;

        /// <summary>
        /// The maximum
        /// </summary>
        public readonly float max;

        /// <summary>
        /// Initializes a new instance of the <see cref="RangeXAttribute"/> class.
        /// </summary>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        public RangeXAttribute(float min, float max)
        {
            this.min = min;
            this.max = max;
        }

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
