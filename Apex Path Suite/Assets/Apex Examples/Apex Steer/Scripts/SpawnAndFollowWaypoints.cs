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

    [AddComponentMenu("Apex/Examples/Spawn And Follow Waypoints", 1023)]
    public class SpawnAndFollowWaypoints : MonoBehaviour
    {
        public GameObject orangeMold;
        public GameObject greenMold;
        public GameObject blueMold;

        private void Start()
        {
            //Get the waypoint transforms
            var waypoints = this.GetComponentsInChildren<Transform>().Skip(1).ToArray();

            //Create the orange group. In this example we just create an empty grouping and add members to it
            var grpOrange = GroupingManager.CreateGrouping<IUnitFacade>();
            for (int i = 0; i < 20; i++)
            {
                var agent = Spawn(this.orangeMold, waypoints[0].position);
                grpOrange.Add(agent);
            }

            IFormation formation = new FormationGrid(1.5f);
            grpOrange.SetFormation(formation);

            for (int i = 1; i < 10; i++)
            {
                grpOrange.MoveTo(waypoints[i % waypoints.Length].position, true);
            }

            //Create the green group.
            var grpGreen = GroupingManager.CreateGroup<IUnitFacade>(9);
            for (int i = 0; i < 9; i++)
            {
                var agent = Spawn(this.greenMold, waypoints[3].position);
                grpGreen.Add(agent);
            }

            formation = new FormationEllipsoid(1.5f);
            grpGreen.SetFormation(formation);

            for (int i = 2; i > -10; i--)
            {
                var idx = Math.Abs((i + waypoints.Length) % waypoints.Length);
                grpGreen.MoveTo(waypoints[idx].position, true);
            }

            //Spawn a solo unit
            var solo = Spawn(this.blueMold, waypoints[4].position);
            for (int i = 0; i < 17; i++)
            {
                solo.MoveTo(waypoints[i % waypoints.Length].position, true);
            }
        }

        private IUnitFacade Spawn(GameObject mold, Vector3 pos)
        {
            var agent = Instantiate(mold, pos, Quaternion.identity) as GameObject;
            agent.SetActive(true);
            return agent.GetUnitFacade();
        }
    }
}
