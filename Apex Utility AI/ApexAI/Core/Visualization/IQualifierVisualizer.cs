/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Visualization
{
    internal interface IQualifierVisualizer : IQualifier
    {
        /// <summary>
        /// Gets the parent.
        /// </summary>
        /// <value>
        /// The parent.
        /// </value>
        SelectorVisualizer parent
        {
            get;
        }

        /// <summary>
        /// Gets the qualifier.
        /// </summary>
        /// <value>
        /// The qualifier.
        /// </value>
        IQualifier qualifier
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the qualifier is the current high scorer.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is high scorer; otherwise, <c>false</c>.
        /// </value>
        bool isHighScorer
        {
            get;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this qualifier has a break point set.
        /// </summary>
        /// <value>
        /// <c>true</c> if this qualifier has a break point set; otherwise, <c>false</c>.
        /// </value>
        bool isBreakPoint
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this qualifier's break point was hit.
        /// </summary>
        /// <value>
        /// <c>true</c> if this qualifier's break point was hit; otherwise, <c>false</c>.
        /// </value>
        bool breakPointHit
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the breakpoint condition.
        /// </summary>
        /// <value>
        /// The breakpoint condition.
        /// </value>
        BreakpointCondition breakpointCondition
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the last score.
        /// </summary>
        /// <value>
        /// The last score.
        /// </value>
        float? lastScore
        {
            get;
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        void Init();

        /// <summary>
        /// Resets this instance.
        /// </summary>
        void Reset();
    }
}
