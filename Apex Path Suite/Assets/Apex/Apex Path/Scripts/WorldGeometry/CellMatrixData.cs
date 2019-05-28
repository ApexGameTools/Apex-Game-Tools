/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    using System;
    using System.Collections.Generic;
    using Apex.Services;
    using Apex.Steering;
    using UnityEngine;

    /// <summary>
    /// Data class for encapsulating and storing grid data.
    /// </summary>
    public abstract class CellMatrixData : ScriptableObject
    {
        [HideInInspector]
        [SerializeField]
        private float[] _heights;

        [HideInInspector]
        [SerializeField]
        private int[] _heightIndices;

        [HideInInspector]
        [SerializeField]
        private int _heightEntries;

        [HideInInspector]
        [SerializeField]
        private int[] _blockedIndexes;

        /// <summary>
        /// Creates a data instance from the specified configuration.
        /// </summary>
        /// <param name="matrix">The matrix to store.</param>
        /// <returns>The data instance</returns>
        public static CellMatrixData Create(CellMatrix matrix)
        {
            CellMatrixData data;

            var c = matrix[0, 0];
            if (c is RichCell)
            {
                data = ScriptableObject.CreateInstance<RichCellData>();
            }
            else if (c is FlatCell)
            {
                data = ScriptableObject.CreateInstance<FlatCellData>();
            }
            else
            {
                data = ScriptableObject.CreateInstance<StandardCellData>();
            }

            data.Initialize(matrix);

            return data;
        }

        /// <summary>
        /// Updates the data with the new state of the matrix.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        public void Refresh(CellMatrix matrix)
        {
            Initialize(matrix);
        }

        internal DataAccessor GetAccessor()
        {
            PrepareForAccess();
            return new DataAccessor(this);
        }

        /// <summary>
        /// Prepares for initialization.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        protected abstract void PrepareForInitialization(CellMatrix matrix);

        /// <summary>
        /// Records the cell data.
        /// </summary>
        /// <param name="c">The cell.</param>
        /// <param name="cellIdx">Index of the cell.</param>
        protected abstract void RecordCellData(Cell c, int cellIdx);

        /// <summary>
        /// Injects the cell data.
        /// </summary>
        /// <param name="c">The cell.</param>
        /// <param name="cellIdx">Index of the cell.</param>
        protected abstract void InjectCellData(Cell c, int cellIdx);

        /// <summary>
        /// Prepares data for access.
        /// </summary>
        protected virtual void PrepareForAccess()
        {
        }

        /// <summary>
        /// Finalizes the initialization.
        /// </summary>
        protected virtual void FinalizeInitialization()
        {
        }

        /// <summary>
        /// Shrinks the specified array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array">The array.</param>
        /// <param name="size">The size.</param>
        /// <returns>The array shrunk to the specified size</returns>
        protected T[] Shrink<T>(T[] array, int size)
        {
            var tmp = new T[size];
            Array.Copy(array, tmp, size);
            return tmp;
        }

        private void Initialize(CellMatrix cellMatrix)
        {
            var sizeX = cellMatrix.columns;
            var sizeZ = cellMatrix.rows;

            var matrix = cellMatrix.rawMatrix;

            PrepareForInitialization(cellMatrix);

            var blockedIndexsList = new List<int>();
            for (int x = 0; x < sizeX; x++)
            {
                for (int z = 0; z < sizeZ; z++)
                {
                    var arrIdx = (z * sizeX) + x;

                    var cell = matrix[x, z];

                    if (!cell.IsWalkableToAny())
                    {
                        blockedIndexsList.Add(arrIdx);
                    }

                    RecordCellData(cell, arrIdx);
                }
            }

            FinalizeInitialization();

            _blockedIndexes = blockedIndexsList.ToArray();

            if (GameServices.heightStrategy.heightMode == HeightSamplingMode.HeightMap)
            {
                var heightSize = cellMatrix.heightMapSize;
                var heightX = heightSize.x;
                var heightZ = heightSize.z;

                _heightEntries = cellMatrix.heightMapEntries;
                _heights = new float[(heightX * heightZ) + 1];
                _heightIndices = new int[(heightX * heightZ) + 1];

                int indicesCount = 0;
                float lastHeight = float.NegativeInfinity;
                for (int x = 0; x < heightX; x++)
                {
                    for (int z = 0; z < heightZ; z++)
                    {
                        var arrIdx = (x * heightZ) + z;

                        var curHeight = cellMatrix.SampleHeight(x, z);
                        if (curHeight != lastHeight)
                        {
                            lastHeight = curHeight;
                            _heights[indicesCount] = curHeight;
                            _heightIndices[indicesCount++] = arrIdx;
                        }
                    }
                }

                _heights[indicesCount] = 0f;
                _heightIndices[indicesCount++] = (heightZ * heightX);

                _heights = Shrink(_heights, indicesCount);
                _heightIndices = Shrink(_heightIndices, indicesCount);
            }
        }

        internal class DataAccessor
        {
            private CellMatrixData _data;
            private HashSet<int> _blockedLookup;
            private int _heightIdx;

            internal DataAccessor(CellMatrixData data)
            {
                _data = data;
                _blockedLookup = new HashSet<int>(_data._blockedIndexes);
            }

            internal int heightEntries
            {
                get { return _data._heightEntries; }
            }

            internal float GetHeight(int idx)
            {
                var indices = _data._heightIndices;
                if (idx >= indices[_heightIdx])
                {
                    _heightIdx++;
                }

                return _data._heights[_heightIdx - 1];
            }

            internal void InjectData(Cell c, int cellIdx)
            {
                _data.InjectCellData(c, cellIdx);
            }

            internal bool IsPermaBlocked(int idx)
            {
                return _blockedLookup.Contains(idx);
            }
        }
    }
}
