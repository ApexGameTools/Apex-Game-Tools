#pragma warning disable 1591
namespace Apex.Examples.SceneSpecific.CustomBlockDetection
{
    using Apex.WorldGeometry;

    public class OddBlockDetector : IBlockDetector
    {
        public static readonly OddBlockDetector instance = new OddBlockDetector();

        public bool IsBlocked(CellMatrix matrix, UnityEngine.Vector3 position, float blockThreshold)
        {
            var x = (int)position.x;
            var z = (int)position.z;

            return (x % 2 != 0) ^ (z % 2 != 0);
        }
    }
}
