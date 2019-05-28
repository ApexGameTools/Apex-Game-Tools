namespace Apex.Examples.Misc
{
    using System.Collections;
    using Apex.Common;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// An alternative example of <see cref="DoorSlider"/>. This one operates on the door frame instead of the door, which gives a better behavior in most cases.
    /// </summary>
    [AddComponentMenu("Apex/Examples/Door Slider Alternative", 1004)]
    [RequireComponent(typeof(Collider))]
    public class DoorSliderTakeTwo : AttributeSensitiveTrigger
    {
        /// <summary>
        /// The door to control
        /// </summary>
        public Transform door;

        /// <summary>
        /// The door frame
        /// </summary>
        public DynamicObstacle doorFrame;

        /// <summary>
        /// The speed in seconds, by which the door will open and close
        /// </summary>
        public float speedInSeconds = 1.0f;

        private Slider _slider;

        private void Awake()
        {
            if (this.door == null || this.doorFrame == null)
            {
                Debug.LogWarning("You must assign a door and a frame to the slider.");
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

            //Here we simple toggle the enabled state of the door obstacle, so as it starts to open it will appear open and vice versa
            //this.doorFrame.enabled = (dir == -1);
            this.doorFrame.Toggle(dir == -1);

            while (_slider.MoveNext(this.door.transform))
            {
                yield return null;
            }
        }

        private class Slider
        {
            private float _moveDistance;
            private float _currentlyMoved;
            private float _startTime;
            private float _step;
            private float _speedInSeconds;
            private int _direction;

            public Slider(float moveDistance, float speedInSeconds)
            {
                _moveDistance = moveDistance;
                _currentlyMoved = moveDistance;
                _speedInSeconds = speedInSeconds;
                _step = 1.0f;
            }

            public bool SetDirection(int direction)
            {
                if (_direction == direction)
                {
                    return false;
                }

                _direction = direction;

                var stepDiff = 1 - _step;
                _startTime = Time.time - (stepDiff * _speedInSeconds);

                _currentlyMoved = _moveDistance - _currentlyMoved;
                return true;
            }

            public bool MoveNext(Transform door)
            {
                if (_currentlyMoved >= _moveDistance)
                {
                    _step = 1.0f;
                    return false;
                }

                _step = (Time.time - _startTime) / _speedInSeconds;
                var diff = Mathf.SmoothStep(0.0f, _moveDistance, _step) - _currentlyMoved;
                _currentlyMoved += diff;

                door.position += door.forward * diff * _direction;

                return true;
            }
        }
    }
}
