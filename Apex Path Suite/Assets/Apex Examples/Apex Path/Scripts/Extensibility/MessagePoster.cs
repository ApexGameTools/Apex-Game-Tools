namespace Apex.Examples.Extensibility
{
    using Apex.Common;
    using Apex.Services;
    using UnityEngine;

    /// <summary>
    /// An example message poster
    /// </summary>
    [AddComponentMenu("Apex/Examples/Extensibility/Message Poster", 1003)]
    public class MessagePoster : AttributedComponent
    {
        /// <summary>
        /// Called when attributes changed.
        /// </summary>
        /// <param name="previous">The previous attributes, before the change.</param>
        protected override void OnAttributesChanged(AttributeMask previous)
        {
            GameServices.messageBus.Post(new MessageAttributesChanged(this));
        }
    }
}
