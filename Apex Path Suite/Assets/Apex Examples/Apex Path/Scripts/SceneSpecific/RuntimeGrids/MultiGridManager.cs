/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Examples.SceneSpecific.RuntimeGrids
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Apex.LoadBalancing;
    using Apex.WorldGeometry;
    using Units;
    using UnityEngine;


    public class MultiGridManager : MonoBehaviour
    {
        public bool createGrids;

        public GameObject premadeGrids;
        public GameObject gridHost;
        public Collider ground;

        public float gridSize = 32f;
        public float cellSize = 2f;

        public GameObject unitMold;
        public Vector3 spawnPosition;

        private IUnitFacade _unit;
        private List<GridComponent> _initializedGrids;
        private List<GridComponent> _initializingGrids;

        private void Start()
        {
            _initializedGrids = new List<GridComponent>(9);
            _initializingGrids = new List<GridComponent>(9);

            //Build grids
            if (createGrids)
            {
                premadeGrids.SetActive(false);
                BuildGrids(ground.bounds);
            }
            else
            {
                premadeGrids.SetActive(true);
            }

            //Initialize the first grids and spawn the unit
            InitializeGrids(spawnPosition, SpawnUnit);

            //Schedule the initialization controller
            LoadBalancer.defaultBalancer.Execute(InitializeGrids, 2f, true);
        }

        private void BuildGrids(Bounds b)
        {
            if (gridSize % cellSize != 0 || gridSize % cellSize != 0)
            {
                Debug.LogError("Grid Width and Grid Height must be a multiple of Cell Size");
            }

            var gridColumns = Mathf.FloorToInt(b.size.x / gridSize);
            var gridRows = Mathf.FloorToInt(b.size.z / gridSize);

            var baseOrigin = b.min + new Vector3(gridSize * .5f, b.size.y, gridSize * .5f);
            var offset = Vector3.zero;

            var cfg = new GridConfig
            {
                cellSize = this.cellSize,
                sizeX = (int)(gridSize / cellSize),
                sizeZ = (int)(gridSize / cellSize),
                automaticConnections = true
            };

            for (int x = 0; x < gridColumns; x++)
            {
                for (int z = 0; z < gridRows; z++)
                {
                    offset.x = x * gridSize;
                    offset.z = z * gridSize;
                    cfg.origin = baseOrigin + offset;

                    GridComponent.Create(this.gridHost, cfg);
                }
            }
        }

        private bool InitializeGrids(float ignored)
        {
            InitializeGrids(_unit.position, null);
            return true;
        }

        private void InitializeGrids(Vector3 position, Action callback)
        {
            //Look for uninitialized grids within a square radius of a 1½ grids size. This could be any value.
            var b = new Bounds(position, new Vector3(this.gridSize * 1.5f, 0f, this.gridSize * 1.5f));

            //This can be optimized for less allocation, but is kept simple to show the idea
            var grids = GridManager.instance.GetGridComponents(b).ToArray();

            //Disable grids that are no longer inside the bounds. This is optional. Doing it will free memory but also cause GC.
            var toDisable = _initializedGrids.Except(grids);
            foreach (var gc in toDisable)
            {
                gc.Disable(4);
            }

            var gridCount = grids.Length;
            for (int i = 0; i < gridCount; i++)
            {
                //Don't init grids that are already initialized or initializing
                if (grids[i].grid != null || _initializingGrids.Contains(grids[i]))
                {
                    continue;
                }

                _initializingGrids.Add(grids[i]);
            }

            //Init the grids that need it and do a callback once all are done.
            int counter = 0;
            gridCount = _initializingGrids.Count;
            for (int i = 0; i < gridCount; i++)
            {
                _initializingGrids[i].Initialize(5, (gc) =>
                {
                    _initializingGrids.Remove(gc);
                    _initializedGrids.Add(gc);

                    if (callback != null && ++counter == gridCount)
                    {
                        callback();
                    }
                });
            }
        }

        private void SpawnUnit()
        {
            var go = Instantiate(this.unitMold, this.spawnPosition, Quaternion.identity) as GameObject;
            go.SetActive(true);

            _unit = go.GetUnitFacade();
        }
    }
}
