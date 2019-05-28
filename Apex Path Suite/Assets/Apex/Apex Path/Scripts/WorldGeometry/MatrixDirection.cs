/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    using Apex.DataStructures;

    /// <summary>
    /// Represents directions within a matrix as deltas
    /// </summary>
    public static class MatrixDirection
    {
        /// <summary>
        /// No direction
        /// </summary>
        public static readonly VectorXZ None = new VectorXZ(0, 0);

        /// <summary>
        /// Down
        /// </summary>
        public static readonly VectorXZ Down = new VectorXZ(0, -1);

        /// <summary>
        /// Up
        /// </summary>
        public static readonly VectorXZ Up = new VectorXZ(0, 1);

        /// <summary>
        /// Down left
        /// </summary>
        public static readonly VectorXZ DownLeft = new VectorXZ(-1, -1);

        /// <summary>
        /// Down right
        /// </summary>
        public static readonly VectorXZ DownRight = new VectorXZ(1, -1);

        /// <summary>
        /// Up left
        /// </summary>
        public static readonly VectorXZ UpLeft = new VectorXZ(-1, 1);

        /// <summary>
        /// Up right
        /// </summary>
        public static readonly VectorXZ UpRight = new VectorXZ(1, 1);

        /// <summary>
        /// Left
        /// </summary>
        public static readonly VectorXZ Left = new VectorXZ(-1, 0);

        /// <summary>
        /// Right
        /// </summary>
        public static readonly VectorXZ Right = new VectorXZ(1, 0);

        /// <summary>
        /// All the directions ordered from the bottom left corner to the top right corner moving across then up.
        /// ____________
        /// |_6_|_7_|_8_|
        /// |_3_|_4_|_5_|
        /// |_0_|_1_|_2_|
        /// </summary>
        public static readonly VectorXZ[] Directions = new VectorXZ[] { DownLeft, Down, DownRight, Left, None, Right, UpLeft, Up, UpRight };
    }
}
