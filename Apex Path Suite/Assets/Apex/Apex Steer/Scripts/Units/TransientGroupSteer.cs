/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Units
{
    using Apex.Steering;

    /// <summary>
    /// Base class for temporary unit groups.
    /// </summary>
    public abstract partial class TransientGroup<T>
    {
        /// <summary>
        /// Gets the current formation.
        /// </summary>
        /// <value>
        /// The current formation.
        /// </value>
        public virtual IFormation currentFormation
        {
            get { return null; }
        }

        /// <summary>
        /// Sets the currently active formation.
        /// </summary>
        /// <param name="formation">The desired formation.</param>
        public virtual void SetFormation(IFormation formation)
        {
            /* NOOP by default */
        }

        /// <summary>
        /// Clears the currently set formation.
        /// </summary>
        public virtual void ClearFormation()
        {
            SetFormation(null);
        }
    }
}
