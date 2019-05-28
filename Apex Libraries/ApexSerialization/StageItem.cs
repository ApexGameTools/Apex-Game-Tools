namespace Apex.Serialization
{
    /// <summary>
    /// Base class for all stage items.
    /// </summary>
    public abstract class StageItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StageItem"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        protected StageItem(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the parent.
        /// </summary>
        /// <value>
        /// The parent.
        /// </value>
        public StageContainer parent
        {
            get;
            internal set;
        }

        internal StageItem next
        {
            get;
            set;
        }

        /// <summary>
        /// Removes the element from its parent. If not parented nothing will happen.
        /// </summary>
        public void Remove()
        {
            if (this.parent != null)
            {
                this.parent.Remove(this);
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Concat(this.name, " (", this.GetType().Name, ")");
        }
    }
}
