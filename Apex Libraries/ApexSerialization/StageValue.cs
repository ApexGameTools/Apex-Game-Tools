/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Serialization
{
    using Utilities;

    /// <summary>
    /// Staged representation of a simple value.
    /// </summary>
    /// <seealso cref="Apex.Serialization.StageItem" />
    public class StageValue : StageItem
    {
        private string _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="StageValue"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <param name="isText">Whether of not the value represents textual content.</param>
        internal StageValue(string name, string value, bool isText)
            : base(name)
        {
            this.value = value;
            this.isText = isText;
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string value
        {
            get
            {
                return _value;
            }

            set
            {
                Ensure.ArgumentNotNull(value, "value");
                _value = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is text.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is text; otherwise, <c>false</c>.
        /// </value>
        public bool isText
        {
            get;
            private set;
        }
    }
}
