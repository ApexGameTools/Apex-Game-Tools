#pragma warning disable 1591
namespace Apex.Examples.SceneSpecific.CustomBlockDetection
{
    using Apex.WorldGeometry;
    using UnityEngine;

    public class RuntimeGrid : MonoBehaviour
    {
        private void Start()
        {
            var go = this.gameObject;

            //Setup the configuration. You only need to set the values you want different from the defaults that you can see on the GridConfig class.
            var gridCfg = new GridConfig
            {
                cellSize = 1f,
                sizeX = 32,
                sizeZ = 32,
                origin = go.transform.position,
                obstacleAndGroundDetection = ColliderDetectionMode.Custom,
                obstacleAndGroundDetector = OddBlockDetector.instance
            };

            //Create the grid instance
            var grid = GridComponent.Create(go, gridCfg);

            //Initialize the grid
            grid.Initialize(10, g =>
            {
                Debug.Log("Initialization Done");
            });
        }
    }
}
