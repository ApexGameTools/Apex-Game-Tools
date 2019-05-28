/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex
{
    using System;

    /// <summary>
    /// Basic representation of compare operators
    /// </summary>
    [Flags]
    public enum CompareOperator
    {
        /// <summary>
        /// No comparison, e.g. force a false return value
        /// </summary>
        None = 0,

        /// <summary>
        /// A &lt; B
        /// </summary>
        LessThan = 1 << 0,

        /// <summary>
        /// A &lt;= B
        /// </summary>
        LessThanOrEquals = 1 << 1,

        /// <summary>
        /// A == B
        /// </summary>
        Equals = 1 << 2,

        /// <summary>
        /// A != B
        /// </summary>
        NotEquals = 1 << 3,

        /// <summary>
        /// A &gt;= B
        /// </summary>
        GreaterThanOrEquals = 1 << 4,

        /// <summary>
        /// A &gt; B
        /// </summary>
        GreaterThan = 1 << 5
    }
}