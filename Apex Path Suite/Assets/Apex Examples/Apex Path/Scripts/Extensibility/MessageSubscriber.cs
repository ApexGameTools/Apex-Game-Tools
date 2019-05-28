#pragma warning disable 1591
namespace Apex.Examples.Extensibility
{
    using Apex.Services;
    using UnityEngine;

    [AddComponentMenu("Apex/Examples/Extensibility/Message Subscriber", 1004)]
    public class MessageSubscriber : ExtendedMonoBehaviour, IHandleMessage<MessageAttributesChanged>
    {
        public void Handle(MessageAttributesChanged message)
        {
            //We only respond to these messages if its a component that sends it
            var sender = message.entity as Component;
            if (sender != null)
            {
                Debug.Log(string.Format("The entity {0} changed its attributes to {1}.", sender.gameObject.name, message.entity.attributes));
            }
        }

        protected override void OnStartAndEnable()
        {
            GameServices.messageBus.Subscribe(this);
        }

        private void OnDisable()
        {
            GameServices.messageBus.Unsubscribe(this);
        }
    }
}
