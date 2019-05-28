#pragma warning disable 1591
namespace Apex.Examples.SceneSpecific.RequestPreProcessing
{
    using Apex.DataStructures;
    using UnityEngine;

    public static class SafeHavens
    {
        private static readonly RectangleXZ[] _havens = new[]
        {
            new RectangleXZ(new Vector3(-10f, 0, 10f), 6f, 4f),
            new RectangleXZ(new Vector3(-12f, 0, -12f), 4f, 4f),
            new RectangleXZ(new Vector3(10f, 0, -11f), 4f, 6f)
        };

        public static bool IsSafe(Vector3 pos)
        {
            for (int i = 0; i < _havens.Length; i++)
            {
                if (_havens[i].Contains(pos))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
