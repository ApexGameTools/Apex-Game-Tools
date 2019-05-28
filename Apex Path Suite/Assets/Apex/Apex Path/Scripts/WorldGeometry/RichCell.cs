/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Apex.Common;
    using Apex.Units;
    using UnityEngine;

    /// <summary>
    /// Represents a cell with rich height data, i.e. data on slope angle, climb and drop heights.
    /// </summary>
    public class RichCell : Cell
    {
        /// <summary>
        /// The cell factory
        /// </summary>
        public static readonly ICellFactory factory = new RichCellFactory();

        private HeightData[] _heightData;
        private HeightData _worstCase;

        /// <summary>
        /// Initializes a new instance of the <see cref="RichCell"/> class.
        /// </summary>
        /// <param name="parent">The cell matrix that owns this cell.</param>
        /// <param name="position">The position.</param>
        /// <param name="matrixPosX">The matrix position x.</param>
        /// <param name="matrixPosZ">The matrix position z.</param>
        /// <param name="blocked">if set to <c>true</c> the cell will appear permanently blocked.</param>
        public RichCell(CellMatrix parent, Vector3 position, int matrixPosX, int matrixPosZ, bool blocked)
            : base(parent, position, matrixPosX, matrixPosZ, blocked)
        {
            //While a bit redundant (e.g. index 4 being self) it makes it faster to index
            _heightData = new HeightData[9];
            for (int i = 0; i < 9; i++)
            {
                //We use this to indicate an uninitialized state
                _heightData[i].slope = 100f;
            }

            //Worst case scenario for checking all directions
            _worstCase = new HeightData();
        }

        /// <summary>
        /// Determines whether the cell is walkable from all directions.
        /// </summary>
        /// <param name="unitProps">The unit properties.</param>
        /// <returns><c>true</c> if the cell is walkable, otherwise <c>false</c></returns>
        public override bool IsWalkableFromAllDirections(IUnitProperties unitProps)
        {
            if (!IsWalkableWithClearance(unitProps))
            {
                return false;
            }

            var uc = unitProps.heightNavigationCapability;

            return ((uc.maxClimbHeight >= _worstCase.dropHeight && uc.maxDropHeight >= _worstCase.climbHeight) || uc.maxSlopeAngle >= _worstCase.slope);
        }

        /// <summary>
        /// Determines whether the cell is walkable from the specified neighbour. This check does not take unit size into consideration.
        /// </summary>
        /// <param name="neighbour">The neighbour.</param>
        /// <param name="unitProps">The unit properties.</param>
        /// <returns>
        ///   <c>true</c> if the cell is walkable, otherwise <c>false</c>
        /// </returns>
        public override bool IsWalkableFrom(IGridCell neighbour, IUnitProperties unitProps)
        {
            if (!IsWalkable(unitProps.attributes))
            {
                return false;
            }

            var dx = Mathf.Clamp(neighbour.matrixPosX - this.matrixPosX, -1, 1);
            var dz = Mathf.Clamp(neighbour.matrixPosZ - this.matrixPosZ, -1, 1);

            var idx = (dx + (3 * dz) + 4);

            var uc = unitProps.heightNavigationCapability;
            var d = _heightData[idx];

            //We compare to opposites since this is from the neighbour's point of view
            return ((uc.maxClimbHeight >= d.dropHeight && uc.maxDropHeight >= d.climbHeight) || uc.maxSlopeAngle >= d.slope);
        }

        /// <summary>
        /// Determines whether the cell is walkable from the specified neighbour.
        /// </summary>
        /// <param name="neighbour">The neighbour.</param>
        /// <param name="unitProps">The unit properties.</param>
        /// <returns><c>true</c> if the cell is walkable, otherwise <c>false</c></returns>
        public override bool IsWalkableFromWithClearance(IGridCell neighbour, IUnitProperties unitProps)
        {
            if (!IsWalkableWithClearance(unitProps))
            {
                return false;
            }

            var dx = Mathf.Clamp(neighbour.matrixPosX - this.matrixPosX, -1, 1);
            var dz = Mathf.Clamp(neighbour.matrixPosZ - this.matrixPosZ, -1, 1);

            var idx = (dx + (3 * dz) + 4);

            var uc = unitProps.heightNavigationCapability;
            var d = _heightData[idx];

            //We compare to opposites since this is from the neighbour's point of view
            return ((uc.maxClimbHeight >= d.dropHeight && uc.maxDropHeight >= d.climbHeight) || uc.maxSlopeAngle >= d.slope);
        }

        internal bool HasInitializedHeightData(int dx, int dz)
        {
            var idx = (dx + (3 * dz) + 4);

            return _heightData[idx].slope < 100f;
        }

        internal void SetHeightData(int dx, int dz, HeightData data)
        {
            var idx = (dx + (3 * dz) + 4);

            _heightData[idx] = data;
        }

        internal void SetHeightData(int idx, HeightData data)
        {
            _heightData[idx] = data;
        }

        internal HeightData[] GetHeightData()
        {
            return _heightData;
        }

        internal void CalculateWorst()
        {
            for (int i = 0; i < 9; i++)
            {
                if (i == 4)
                {
                    i++;
                }

                var d = _heightData[i];

                if (_worstCase.dropHeight < d.dropHeight)
                {
                    _worstCase.dropHeight = d.dropHeight;
                }

                if (_worstCase.climbHeight < d.climbHeight)
                {
                    _worstCase.climbHeight = d.climbHeight;
                }

                if (_worstCase.slope < d.slope)
                {
                    _worstCase.slope = d.slope;
                }
            }
        }

        internal override void EnsureDataForEditor()
        {
            //Since height settings are saved sparsely half are on neighbours and since this cell has only just been created,
            //if the neighbour was created before it has not been able to update this cell, so take care of that
            InjectDataFrom(0, 1);
            InjectDataFrom(1, 1);
            InjectDataFrom(1, 0);
            InjectDataFrom(1, -1);
        }

        //Debugging purposes only
        internal HeightData GetHeightData(IGridCell neighbour)
        {
            var dx = Mathf.Clamp(neighbour.matrixPosX - this.matrixPosX, -1, 1);
            var dz = Mathf.Clamp(neighbour.matrixPosZ - this.matrixPosZ, -1, 1);

            var idx = (dx + (3 * dz) + 4);

            return _heightData[idx];
        }

        private void InjectDataFrom(int dx, int dz)
        {
            var n = this.parent[this.matrixPosX + dx, this.matrixPosZ + dz] as RichCell;
            if (n != null)
            {
                var idx = (dx + (3 * dz) + 4);
                var nidx = (-dx + (3 * -dz) + 4);

                _heightData[idx] = n._heightData[nidx];
            }
        }

        /// <summary>
        /// Data structure for storing height data on the cell
        /// </summary>
        [Serializable]
        public struct HeightData
        {
            /// <summary>
            /// The slope angle
            /// </summary>
            public float slope;

            /// <summary>
            /// The climb height
            /// </summary>
            public float climbHeight;

            /// <summary>
            /// The drop height
            /// </summary>
            public float dropHeight;

            /// <summary>
            /// Initializes a new instance of the <see cref="HeightData"/> struct.
            /// </summary>
            /// <param name="slope">The slope.</param>
            /// <param name="climbHeight">Height of the climb.</param>
            /// <param name="dropHeight">Height of the drop.</param>
            public HeightData(float slope, float climbHeight, float dropHeight)
            {
                this.slope = slope;
                this.climbHeight = climbHeight;
                this.dropHeight = dropHeight;
            }
        }

        private class RichCellFactory : ICellFactory
        {
            public Cell Create(CellMatrix parent, Vector3 position, int matrixPosX, int matrixPosZ, bool blocked)
            {
                return new RichCell(parent, position, matrixPosX, matrixPosZ, blocked);
            }
        }
    }
}
