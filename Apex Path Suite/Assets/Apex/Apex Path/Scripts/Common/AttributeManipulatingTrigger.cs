/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Common
{
    using UnityEngine;

    /// <summary>
    /// A trigger behaviour that can apply and/or remove one or more attributes from an entity when the entity enters and/or exits the trigger area.
    /// </summary>
    [AddComponentMenu("Apex/Game World/Attribute Manipulating Trigger", 1028)]
    [ApexComponent("Triggers")]
    public class AttributeManipulatingTrigger : MonoBehaviour
    {
        /// <summary>
        /// When the trigger behaviour should happen, on entry, exit or both.
        /// </summary>
        [Tooltip("When the trigger behaviour should happen, on entry, exit or both.")]
        public Trigger updateOn = Trigger.Both;

        [SerializeField, AttributeProperty("Applies", "The attribute mask applied when this trigger is triggered.")]
        private int _applies;

        [SerializeField, AttributeProperty("Removes", "The attribute mask removed when this trigger is triggered.")]
        private int _removes;

        /// <summary>
        /// The criteria for when the trigger should 'trigger'
        /// </summary>
        public enum Trigger
        {
            /// <summary>
            /// Triggers on enter
            /// </summary>
            OnEnter = 1,

            /// <summary>
            /// Triggers on exit
            /// </summary>
            OnExit = 2,

            /// <summary>
            /// Triggers on both enter and exit
            /// </summary>
            Both = OnEnter | OnExit
        }

        /// <summary>
        /// Gets or sets the attributes to apply to units when the trigger triggers.
        /// </summary>
        public AttributeMask applies
        {
            get { return _applies; }
            set { _applies = value; }
        }

        /// <summary>
        /// Gets or sets the attributes to remove to units when the trigger triggers.
        /// </summary>
        public AttributeMask removes
        {
            get { return _removes; }
            set { _removes = value; }
        }

        private static void Apply(Collider other, int apply, int remove)
        {
            var entity = other.GetComponent<AttributedComponent>();
            if (entity == null)
            {
                return;
            }

            entity.attributes |= apply;
            entity.attributes &= ~remove;
        }

        private void OnTriggerEnter(Collider other)
        {
            if ((this.updateOn & Trigger.OnEnter) > 0)
            {
                Apply(other, _applies, _removes);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (this.updateOn == Trigger.OnExit)
            {
                Apply(other, _applies, _removes);
            }
            else if ((this.updateOn & Trigger.OnExit) > 0)
            {
                Apply(other, _removes, _applies);
            } 
        }
    }
}
