namespace Apex.Examples.Misc
{
    using System.Collections;
    using Apex.Common;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// A trigger component that slides a door open and closed. It only reacts to units with certain attributes.
    /// </summary>
    [AddComponentMenu("Apex/Examples/Door Slider", 1003)]
    [RequireComponent(typeof(Collider))]
    public class DoorSlider : AttributeSensitiveTrigger
    {
        /// <summary>
        /// The door to control
        /// </summary>
        public Transform door;

        /// <summary>
        /// The speed in seconds, by which the door will open and close
        /// </summary>
        public float speedInSeconds = 1.0f;

        private IDynamicObstacle _obstacle;
        private Slider _slider;

        private void Awake()
        {
            if (this.door == null)
            {
                Debug.LogWarning("You must assign a door to the slider.");
                this.enabled = false;
                return;
            }

            _obstacle = this.door.GetComponent<DynamicObstacle>();

            if (_obstacle == null)
            {
                //Of course you could opt to simply add it to the door if missing instead if failing.
                Debug.LogWarning("You must assign a DynamicObstacle component to the door.");
                this.enabled = false;
                return;
            }

            var b = this.door.GetComponent<Renderer>().bounds;

            //move distance is a bit too long if the door is axis aligned, but it'll do for this example
            _slider = new Slider((b.max - b.min).magnitude, this.speedInSeconds);
        }

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
            if (entitiesInTrigger > 1)
            {
                return null;
            }

            return Slide(1);
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
            if (entitiesInTrigger > 0)
            {
                return null;
            }

            return Slide(-1);
        }

        private IEnumerator Slide(int dir)
        {
            if (!_slider.SetDirection(dir))
            {
                yield break;
            }

            while (_slider.MoveNext(this.door.transform))
            {
                yield return null;
            }

            //We only update the door obstacle status when it comes to a rest in either its open or closed state to avoid unnecessary replans
            _obstacle.ActivateUpdates(null, false);
        }
    }
}
