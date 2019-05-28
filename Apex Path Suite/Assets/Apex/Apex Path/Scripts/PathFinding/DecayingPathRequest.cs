/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.PathFinding
{
    using System.Diagnostics;

    /// <summary>
    /// Represents a request that will decay after a certain period of time.
    /// </summary>
    public class DecayingPathRequest : BasicPathRequest
    {
        private int _timeLimit;
        private bool _hardDecay;
        private Stopwatch _w;

        /// <summary>
        /// Initializes a new instance of the <see cref="DecayingPathRequest"/> class.
        /// </summary>
        /// <param name="timeLimitInMilliseconds">The time limit in milliseconds.</param>
        public DecayingPathRequest(int timeLimitInMilliseconds)
        {
            _timeLimit = timeLimitInMilliseconds;
            _w = Stopwatch.StartNew();
        }

        /// <summary>
        /// Gets a value indicating whether this instance has decayed.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has decayed; otherwise, <c>false</c>.
        /// </value>
        public override bool hasDecayed
        {
            get
            {
                return _hardDecay || _w.ElapsedMilliseconds >= _timeLimit;
            }

            set
            {
                _hardDecay = value;
            }
        }
    }
}
