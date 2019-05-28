/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Serialization
{
    using System;

    /// <summary>
    /// Interface for types that handle staging of values. Implementing classes must also have a parameterless constructor.
    /// </summary>
    public interface IStager
    {
        /// <summary>
        /// Gets the types this stager can handle.
        /// </summary>
        /// <value>
        /// The handled types.
        /// </value>
        Type[] handledTypes { get; }

        /// <summary>
        /// Stages the value.
        /// </summary>
        /// <param name="name">The name to give the resulting <see cref="StageItem"/>.</param>
        /// <param name="value">The value to stage</param>
        /// <returns>The element containing the staged value.</returns>
        StageItem StageValue(string name, object value);

        /// <summary>
        /// Unstages the value.
        /// </summary>
        /// <param name="item">The stage item to unstage.</param>
        /// <param name="targetType">Type of the value.</param>
        /// <returns>The unstaged value.</returns>
        object UnstageValue(StageItem item, Type targetType);
    }
}
