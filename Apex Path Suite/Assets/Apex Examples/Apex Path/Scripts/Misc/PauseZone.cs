namespace Apex.Examples.Misc
{
    using Apex.Steering.Components;
    using UnityEngine;

    /// <summary>
    /// A trigger behavior that makes entities that enter it take a breather.
    /// </summary>
    [AddComponentMenu("Apex/Examples/Pause Zone", 1015)]
    public class PauseZone : MonoBehaviour
    {
        /// <summary>
        /// The pause duration
        /// </summary>
        public float pauseForSeconds = 1.0f;

        /// <summary>
        /// Whether to pause indefinitely
        /// </summary>
        public bool pauseIndefintely = false;

        private void OnTriggerEnter(Collider other)
        {
            var steerer = other.GetUnitFacade();
            if (steerer == null)
            {
                return;
            }

            if (this.pauseIndefintely)
            {
                steerer.Wait(null);
            }
            else
            {
                steerer.Wait(this.pauseForSeconds);
            }
        }
    }
}
