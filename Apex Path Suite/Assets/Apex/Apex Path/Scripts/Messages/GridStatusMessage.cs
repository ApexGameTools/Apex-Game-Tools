namespace Apex.Messages
{
    using UnityEngine;

    /// <summary>
    /// Message sent after a grid has been initialized or disabled at runtime.
    /// </summary>
    public class GridStatusMessage
    {
        /// <summary>
        /// The possible statuses
        /// </summary>
        public enum StatusCode
        {
            /// <summary>
            /// The grid was initialized
            /// </summary>
            InitializationComplete,

            /// <summary>
            /// The grid was disabled
            /// </summary>
            DisableComplete
        }

        /// <summary>
        /// Gets or sets the bounds of the grid in question.
        /// </summary>
        public Bounds gridBounds
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public StatusCode status
        {
            get;
            set;
        }
    }
}
