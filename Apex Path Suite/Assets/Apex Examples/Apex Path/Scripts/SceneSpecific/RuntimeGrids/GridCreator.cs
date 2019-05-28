/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Examples.SceneSpecific.RuntimeGrids
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Apex.DataStructures;
    using Apex.WorldGeometry;
    using UnityEngine;

    public class GridCreator : MonoBehaviour
    {
        public GameObject gridHost;

        private void Start()
        {
            //Setup the configuration. You only need to set the values you want different from the defaults that you can see on the GridConfig class.
            var gridCfg = new GridConfig
            {
                cellSize = 2f,
                generateHeightmap = true,
                heightLookupType = HeightLookupType.QuadTree,
                heightLookupMaxDepth = 5,
                lowerBoundary = 1f,
                upperBoundary = 10f,
                obstacleSensitivityRange = 0.5f,
                sizeX = 16,
                sizeZ = 16,
                subSectionsX = 2,
                subSectionsZ = 2,
                subSectionsCellOverlap = 2,
                origin = this.gridHost.transform.position
            };

            //Create the grid instance
            var grid = GridComponent.Create(this.gridHost, gridCfg);

            //Initialize the grid
            grid.Initialize(10, g =>
                {
                    Debug.Log("Initialization Done");
                });
        }
    }
}
