/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Examples.Steering
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Apex.Steering;
    using Apex.Units;
    using UnityEngine;

    [AddComponentMenu("Apex/Examples/Group Agent Spawner", 1007)]
    public class SteerAISpawner : MonoBehaviour
    {
        private SteerPOIManager _poiSource;

        public GameObject agentMold;
        public int groupSize = 10;
        public int waypointIterations = 30;
        public Vector3[] spawnPoints;

        private void Awake()
        {
            _poiSource = GetComponent<SteerPOIManager>();
        }

        private void Start()
        {
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                var randomColor = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                var grp = new DefaultSteeringTransientUnitGroup(this.groupSize);
                for (int j = 0; j < this.groupSize; j++)
                {
                    var agent = Instantiate(this.agentMold, spawnPoints[i], Quaternion.identity) as GameObject;
                    agent.SetActive(true);
                    grp.Add(agent.GetUnitFacade());

                    var renderer = agent.GetComponent<Renderer>();
                    renderer.material.color = randomColor;
                }

                Vector3 lastPOI = _poiSource.GetPOI(Vector3.zero);
                for (int j = 0; j < waypointIterations; j++)
                {
                    grp.MoveTo(lastPOI, true);
                    lastPOI = _poiSource.GetPOI(lastPOI);
                }
            }
        }
    }
}
