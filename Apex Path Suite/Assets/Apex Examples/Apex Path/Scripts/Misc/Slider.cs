namespace Apex.Examples.Misc
{
    using UnityEngine;

    /// <summary>
    /// A basic utility for moving a transform a certain distance, back and forth.
    /// </summary>
    public class Slider
    {
        private float _moveDistance;
        private float _currentlyMoved;
        private float _startTime;
        private float _step;
        private float _speedInSeconds;
        private int _direction;

        /// <summary>
        /// Initializes a new instance of the <see cref="Slider"/> class.
        /// </summary>
        /// <param name="moveDistance">The move distance.</param>
        /// <param name="speedInSeconds">The speed in seconds.</param>
        public Slider(float moveDistance, float speedInSeconds)
        {
            _moveDistance = moveDistance;
            _currentlyMoved = moveDistance;
            _speedInSeconds = speedInSeconds;
            _step = 1.0f;
        }

        /// <summary>
        /// Sets the direction of movement.
        /// </summary>
        /// <param name="direction">The direction.</param>
        /// <returns><c>true</c> if the direction was changed, otherwise <c>false</c></returns>
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

        /// <summary>
        /// Continues a move
        /// </summary>
        /// <param name="obj">The object to move.</param>
        /// <returns><c>true</c> if movement is still ongoing, otherwise <c>false</c></returns>
        public bool MoveNext(Transform obj)
        {
            if (_currentlyMoved >= _moveDistance)
            {
                _step = 1.0f;
                return false;
            }

            _step = (Time.time - _startTime) / _speedInSeconds;
            var diff = Mathf.SmoothStep(0.0f, _moveDistance, _step) - _currentlyMoved;
            _currentlyMoved += diff;

            obj.position += obj.forward * diff * _direction;

            return true;
        }
    }
}
