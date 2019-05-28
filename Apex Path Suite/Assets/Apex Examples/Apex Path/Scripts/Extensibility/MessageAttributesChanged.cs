namespace Apex.Examples.Extensibility
{
    using Apex.Common;

    /// <summary>
    /// An example message
    /// </summary>
    public class MessageAttributesChanged
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageAttributesChanged"/> class.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public MessageAttributesChanged(IHaveAttributes entity)
        {
            this.entity = entity;
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <value>
        /// The entity.
        /// </value>
        public IHaveAttributes entity
        {
            get;
            private set;
        }
    }
}
