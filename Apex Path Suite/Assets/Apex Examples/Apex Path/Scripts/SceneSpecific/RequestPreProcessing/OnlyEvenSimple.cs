/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Examples.SceneSpecific.RequestPreProcessing
{
    using Apex.PathFinding;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// This example pre processor ensures that a unit is never moved to an uneven index cell and always move to the center.
    /// It is of course completely useless besides showing how a preprocessor works.
    /// </summary>
    public class OnlyEvenSimple : MonoBehaviour, IRequestPreProcessor
    {
        private Cell[] _neighbourBuffer = new Cell[8];
        private bool _enabled;

        [SerializeField]
        private int _priority = 1;

        public int priority
        {
            get { return _priority; }
            set { _priority = value; }
        }

        public bool PreProcess(IPathRequest request)
        {
            if (!_enabled)
            {
                return false;
            }

            var grid = GridManager.instance.GetGrid(request.to);
            if (grid == null)
            {
                return false;
            }

            var goal = grid.GetCell(request.to, true);
            if (IsEvenIndex(goal))
            {
                request.to = goal.position;
                return true;
            }

            int count = goal.GetNeighbours(_neighbourBuffer);
            for (int i = 0; i < count; i++)
            {
                if (IsEvenIndex(_neighbourBuffer[i]))
                {
                    request.to = _neighbourBuffer[i].position;
                    return true;
                }
            }

            return false;
        }

        private bool IsEvenIndex(Cell c)
        {
            return c.matrixPosX % 2 == 0 && c.matrixPosZ % 2 == 0;
        }

        private void OnEnable()
        {
            _enabled = true;
        }

        private void OnDisable()
        {
            _enabled = false;
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            var matrix = GridManager.instance.GetGrid(Vector3.zero).cellMatrix;

            var horSize = new Vector3(matrix.bounds.size.x, 0.1f, matrix.cellSize);
            var verSize = new Vector3(matrix.cellSize, 0.1f, matrix.bounds.size.z);
            var horCenter = new Vector3(0f, 0f, matrix.bounds.size.z * 0.5f);
            var verCenter = new Vector3(matrix.bounds.size.x * 0.5f, 0f, 0f);

            Gizmos.color = new Color(255f, 0f, 0f, 0.15f);

            var raw = matrix.rawMatrix;
            for (int x = 1; x < matrix.columns; x += 2)
            {
                verCenter.x = raw[x, 0].position.x;
                Gizmos.DrawCube(verCenter, verSize);
            }

            for (int z = 1; z < matrix.rows; z += 2)
            {
                horCenter.z = raw[0, z].position.z;
                Gizmos.DrawCube(horCenter, horSize);
            }
        }
    }
}
