/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI
{
    /// <summary>
    /// Represents an option and its score.
    /// </summary>
    public struct ScoredOption<TOption>
    {
        private TOption _option;
        private float _score;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScoredOption{TOption}"/> struct.
        /// </summary>
        /// <param name="option">The option.</param>
        /// <param name="score">The score.</param>
        public ScoredOption(TOption option, float score)
        {
            _option = option;
            _score = score;
        }

        /// <summary>
        /// The option
        /// </summary>
        public TOption option
        {
            get { return _option; }
        }

        /// <summary>
        /// The score
        /// </summary>
        public float score
        {
            get { return _score; }
        }
    }
}
