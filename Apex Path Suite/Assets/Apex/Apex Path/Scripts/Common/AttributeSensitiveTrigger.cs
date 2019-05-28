/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Common
{
    using System.Collections;
    using Apex.Units;
    using UnityEngine;

    /// <summary>
    /// Base class for implementing triggers that trigger based on the attributes of the entity that enters the trigger area.
    /// </summary>
    public abstract class AttributeSensitiveTrigger : MonoBehaviour
    {
        /// <summary>
        /// Whether the entity's attributes must be an exact match to trigger this trigger. If false the trigger will trigger if the entity has at least one of the attributes assigned to <see cref="triggeredBy"/>
        /// </summary>
        [Tooltip("Whether the entity's attributes must be an exact match to trigger this trigger. If false the trigger will trigger if the entity has at least one of the attributes assigned to Triggered By")]
        public bool strictMatch;

        /// <summary>
        /// Whether to invert the <see cref="triggeredBy"/> mask, i.e. make it so that the trigger triggers for all but <see cref="triggeredBy"/>
        /// </summary>
        [Tooltip("Whether to invert the Triggered By mask, i.e. make it so that the trigger triggers for all except Triggered By.")]
        public bool invert;

        [SerializeField, AttributeProperty("Triggered By", "The mask if attributes that trigger this trigger.")]
        private int _triggerdBy;
        private int _entitiesInTrigger;

        /// <summary>
        /// Gets or sets the attributes that trigger this trigger.
        /// </summary>
        public AttributeMask triggeredBy
        {
            get { return _triggerdBy; }
            set { _triggerdBy = value; }
        }

        /// <summary>
        /// Called when the trigger is entered.
        /// </summary>
        /// <param name="other">The entity that entered.</param>
        /// <param name="entitiesInTrigger">The entities currently in the trigger.</param>
        /// <returns>Can be implemented as a coroutine (yield). Otherwise just return null.</returns>
        protected abstract IEnumerator OnTriggerEntered(Collider other, int entitiesInTrigger);

        /// <summary>
        /// Called when the trigger is exited.
        /// </summary>
        /// <param name="other">The entity that exited.</param>
        /// <param name="entitiesInTrigger">The entities currently in the trigger.</param>
        /// <returns>Can be implemented as a coroutine (yield). Otherwise just return null.</returns>
        protected abstract IEnumerator OnTriggerExited(Collider other, int entitiesInTrigger);

        private IEnumerator OnTriggerEnter(Collider other)
        {
            if (!IsAttributeMatch(other))
            {
                return null;
            }

            return OnTriggerEntered(other, ++_entitiesInTrigger);
        }

        private IEnumerator OnTriggerExit(Collider other)
        {
            if (!IsAttributeMatch(other))
            {
                return null;
            }

            return OnTriggerExited(other, --_entitiesInTrigger);
        }

        private bool IsAttributeMatch(Collider other)
        {
            var entity = other.GetComponent<AttributedComponent>();
            if (entity == null)
            {
                return false;
            }

            bool triggered = false;

            if (this.strictMatch)
            {
                triggered = (entity.attributes & _triggerdBy) == _triggerdBy;
            }
            else
            {
                triggered = (entity.attributes & _triggerdBy) > 0;
            }

            if (this.invert)
            {
                return !triggered;
            }

            return triggered;
        }
    }
}
