namespace Apex.Examples.Extensibility
{
    using System.Collections;
    using Apex.Common;
    using UnityEngine;

    /// <summary>
    /// An example trigger.
    /// </summary>
    [AddComponentMenu("Apex/Examples/Extensibility/Attribute Sensitive Trigger Example", 1000)]
    public class AttributeSensitiveTriggerExample : AttributeSensitiveTrigger
    {
        /// <summary>
        /// Called when the trigger is entered.
        /// </summary>
        /// <param name="other">The entity that entered.</param>
        /// <param name="entitiesInTrigger">The entities currently in the trigger.</param>
        /// <returns>
        /// Can be implemented as a coroutine (yield). Otherwise just return null.
        /// </returns>
        protected override IEnumerator OnTriggerEntered(Collider other, int entitiesInTrigger)
        {
            //The collider will have the attribute(s) hat match this trigger, so there is no test done to see if its a valid entity.
            Debug.Log(string.Format("{0} has entered, there are now {1} entities inside the trigger.", other.gameObject.name, entitiesInTrigger));

            //Since we have no need for a coroutine, we just return null
            return null;
        }

        /// <summary>
        /// Called when the trigger is exited.
        /// </summary>
        /// <param name="other">The entity that exited.</param>
        /// <param name="entitiesInTrigger">The entities currently in the trigger.</param>
        /// <returns>
        /// Can be implemented as a coroutine (yield). Otherwise just return null.
        /// </returns>
        protected override IEnumerator OnTriggerExited(Collider other, int entitiesInTrigger)
        {
            //The collider will have the attribute(s) hat match this trigger, so there is no test done to see if its a valid entity.
            Debug.Log(string.Format("{0} has exited, there are now {1} entities inside the trigger.", other.gameObject.name, entitiesInTrigger));

            //Since we have no need for a coroutine, we just return null
            return null;
        }
    }
}
