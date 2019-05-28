#pragma warning disable 1591
namespace Apex.Examples.SceneSpecific.CustomBlockDetection
{
    using Apex.WorldGeometry;
    using UnityEngine;

    public class OddBlockDetectorFactory : MonoBehaviour, IBlockDetectorFactory
    {
        public IBlockDetector Create()
        {
            return OddBlockDetector.instance;
        }
    }
}
