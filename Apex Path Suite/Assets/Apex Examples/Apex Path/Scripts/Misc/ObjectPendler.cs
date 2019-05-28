namespace Apex.Examples.Misc
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;

    /// <summary>
    /// A (somewhat naive) behaviour that moves an object between to positions
    /// </summary>
    [AddComponentMenu("Apex/Examples/Object Pendler", 1011)]
    public class ObjectPendler : MonoBehaviour
    {
        /// <summary>
        /// The 'from' position
        /// </summary>
        public Vector3 from;

        /// <summary>
        /// The 'to' position
        /// </summary>
        public Vector3 to;

        /// <summary>
        /// The travel time
        /// </summary>
        public float travelTime;

        private Slider _slider;
        private Position _currentPos;

        /// <summary>
        /// The position
        /// </summary>
        public enum Position
        {
            /// <summary>
            /// From position
            /// </summary>
            From,

            /// <summary>
            /// To position
            /// </summary>
            To
        }

        private void Awake()
        {
            _slider = new Slider((from - to).magnitude, this.travelTime);

            if (this.transform.position.Approximately(from, 0.01f))
            {
                _currentPos = Position.From;
            }
            else if (this.transform.position.Approximately(to, 0.01f))
            {
                _currentPos = Position.To;
            }
            else
            {
                Debug.LogWarning("An object with Object Pendler must start at either of its two positions.");
            }
        }

        /// <summary>
        /// Determines whether the component is at position p.
        /// </summary>
        /// <param name="p">The position.</param>
        /// <returns><c>true</c> if at position, otherwise <c>false</c></returns>
        public bool IsAtPosition(Position p)
        {
            return p == _currentPos;
        }

        /// <summary>
        /// Moves to a position
        /// </summary>
        /// <param name="p">The position to move to.</param>
        /// <param name="cb">The callback to call once the move is complete.</param>
        public void MoveTo(Position p, Action cb)
        {
            if (p == Position.From)
            {
                MoveToFrom(cb);
                _currentPos = Position.From;
            }
            else
            {
                MoveToTo(cb);
                _currentPos = Position.To;
            }
        }

        private void MoveToTo(Action cb)
        {
            var dir = Math.Sign(Vector3.Dot((to - from).normalized, this.transform.forward));

            StartCoroutine(Move(dir, cb));
        }

        private void MoveToFrom(Action cb)
        {
            var dir = Math.Sign(Vector3.Dot((from - to).normalized, this.transform.forward));

            StartCoroutine(Move(dir, cb));
        }

        private IEnumerator Move(int dir, Action cb)
        {
            if (!_slider.SetDirection(dir))
            {
                cb();
                yield break;
            }

            while (_slider.MoveNext(this.transform))
            {
                yield return null;
            }

            cb();
        }
    }
}
