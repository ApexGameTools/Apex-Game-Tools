/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex
{
    /// <summary>
    /// How mask matching is done when evaluating A &amp; B
    /// </summary>
    public enum MaskMatch
    {
        /// <summary>
        /// The masks A and B share no bits.
        /// </summary>
        NoMatch,

        /// <summary>
        /// Partial matching means at least one bit in common between mask A and B.
        /// </summary>
        Partial,

        /// <summary>
        /// Strict matching means that all bits in B must be in A.
        /// </summary>
        Strict,

        /// <summary>
        /// Equals matching means that A and B must be equal, e.g. full match.
        /// </summary>
        Equals
    }
}
