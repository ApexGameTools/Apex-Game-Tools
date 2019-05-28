﻿/* Copyright © 2014 Apex Software. All rights reserved. */
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
    /// Represents a cell with rich height data, i.e. data on slope angle, climb and drop heights and clearance.
    /// </summary>
    public class RichClearanceCell : RichCell, IHaveClearance
    {
        /// <summary>
        /// The cell factory
        /// </summary>
        public static new readonly ICellFactory factory = new RichClearanceCellFactory();

        private float _clearance = float.MaxValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="RichClearanceCell"/> class.
        /// </summary>
        /// <param name="parent">The cell matrix that owns this cell.</param>
        /// <param name="position">The position.</param>
        /// <param name="matrixPosX">The matrix position x.</param>
        /// <param name="matrixPosZ">The matrix position z.</param>
        /// <param name="blocked">if set to <c>true</c> the cell will appear permanently blocked.</param>
        public RichClearanceCell(CellMatrix parent, Vector3 position, int matrixPosX, int matrixPosZ, bool blocked)
            : base(parent, position, matrixPosX, matrixPosZ, blocked)
        {
        }

        /// <summary>
        /// Gets or sets the clearance value of the cell, i.e. the distance to the nearest blocked cell.
        /// </summary>
        public float clearance
        {
            get { return _clearance; }
            set { _clearance = value; }
        }

        /// <summary>
        /// Determines whether the cell is walkable to a certain unit / unit type.
        /// </summary>
        /// <param name="unitProps">The unit properties.</param>
        /// <returns>
        ///   <c>true</c> if the cell is walkable, otherwise <c>false</c>
        /// </returns>
        public override bool IsWalkableWithClearance(IUnitProperties unitProps)
        {
            if (unitProps.radius > _clearance)
            {
                return false;
            }

            return base.IsWalkableWithClearance(unitProps);
        }

        private class RichClearanceCellFactory : ICellFactory
        {
            public Cell Create(CellMatrix parent, Vector3 position, int matrixPosX, int matrixPosZ, bool blocked)
            {
                return new RichClearanceCell(parent, position, matrixPosX, matrixPosZ, blocked);
            }
        }
    }
}
