#pragma warning disable 1591
namespace Apex.Examples.SceneSpecific.RequestPreProcessing
{
    using Apex.DataStructures;
    using Apex.PathFinding;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// This example pre processor ensures that a unit can only move to safe spots.
    /// </summary>
    public class OnlySafeSimple : MonoBehaviour, IRequestPreProcessor
    {
        private bool _enabled;
        private DynamicArray<Cell> _buffer = new DynamicArray<Cell>(50);

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
            if (IsItSafe(goal))
            {
                return false;
            }

            int d = 1;
            do
            {
                _buffer.Clear();
                grid.GetConcentricNeighbours(goal, d++, _buffer);
                var count = _buffer.count;
                for (int i = 0; i < count; i++)
                {
                    if (IsItSafe(_buffer[i]))
                    {
                        request.to = _buffer[i].position;
                        return true;
                    }
                }
            }
            while (_buffer.count > 0);

            return false;
        }

        private bool IsItSafe(Cell c)
        {
            return SafeHavens.IsSafe(c.position);
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

            var cubeSize = new Vector3(matrix.cellSize, 0.1f, matrix.cellSize);

            Gizmos.color = new Color(0f, 255f, 0f, 0.15f);

            var raw = matrix.rawMatrix;
            for (int x = 0; x < matrix.columns; x++)
            {
                for (int z = 0; z < matrix.rows; z++)
                {
                    if (SafeHavens.IsSafe(raw[x, z].position))
                    {
                        Gizmos.DrawCube(raw[x, z].position, cubeSize);
                    }
                }
            }
        }
    }
}
