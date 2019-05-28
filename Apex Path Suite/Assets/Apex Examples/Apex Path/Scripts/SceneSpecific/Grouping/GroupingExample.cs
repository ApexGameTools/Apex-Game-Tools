#pragma warning disable 1591
namespace Apex.Examples.SceneSpecific.Grouping
{
    using System.Collections.Generic;
    using Apex.Units;
    using UnityEngine;

    [AddComponentMenu("Apex/Examples/SceneSpecific/Grouping/Grouping Example", 1003)]
    public class GroupingExample : MonoBehaviour
    {
        public GameObject orangeMold;
        public GameObject greenMold;

        public Transform target;

        private void SpawnGroupOne()
        {
            //Here we simply create a groups manually, which will be the most common for AI units
            var grpOrange = GroupingManager.CreateGroup<IUnitFacade>(3);
            var grpGreen = GroupingManager.CreateGroup<IUnitFacade>(3);

            for (int i = 0; i < 3; i++)
            {
                var go = Instantiate(this.orangeMold, Vector3.zero, Quaternion.identity) as GameObject;
                grpOrange.Add(go.GetUnitFacade());

                go = Instantiate(this.greenMold, Vector3.zero, Quaternion.identity) as GameObject;
                grpGreen.Add(go.GetUnitFacade());
            }

            grpOrange.MoveTo(target.position, false);
            grpGreen.MoveTo(target.position, false);
        }

        private void SpawnGroupTwo()
        {
            var orangeUnits = InstantiateUnits(3, 0);
            var greenUnits = InstantiateUnits(0, 3);

            //You can also create groups from already existing lists
            var grpOrange = GroupingManager.CreateGroup(orangeUnits);
            var grpGreen = GroupingManager.CreateGroup(greenUnits);

            grpOrange.MoveTo(target.position, false);
            grpGreen.MoveTo(target.position, false);
        }

        private void SpawnGroupThree()
        {
            //You can of source also simply create the specific group type directly.
            //If you don't plan to change your strategy at runtime this is likely the best performing option
            var grpOrange = new OnePathForAllGroup(3);
            var grpGreen = new OnePathForAllGroup(3);

            for (int i = 0; i < 3; i++)
            {
                var go = Instantiate(this.orangeMold, Vector3.zero, Quaternion.identity) as GameObject;
                grpOrange.Add(go.GetUnitFacade());

                go = Instantiate(this.greenMold, Vector3.zero, Quaternion.identity) as GameObject;
                grpGreen.Add(go.GetUnitFacade());
            }

            grpOrange.MoveTo(target.position, false);
            grpGreen.MoveTo(target.position, false);
        }

        private void SpawnGrouping()
        {
            var units = InstantiateUnits(3, 3);

            var grouping = GroupingManager.CreateGrouping(units);
            grouping.MoveTo(target.position, false);
        }

        private IEnumerable<IUnitFacade> InstantiateUnits(int orangeCount, int greenCount)
        {
            for (int i = 0; i < orangeCount; i++)
            {
                var go = Instantiate(this.orangeMold, Vector3.zero, Quaternion.identity) as GameObject;
                yield return go.GetUnitFacade();
            }

            for (int i = 0; i < greenCount; i++)
            {
                var go = Instantiate(this.greenMold, Vector3.zero, Quaternion.identity) as GameObject;
                yield return go.GetUnitFacade();
            }
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Q))
            {
                SpawnGroupOne();
            }
            else if (Input.GetKeyUp(KeyCode.W))
            {
                SpawnGroupTwo();
            }
            else if (Input.GetKeyUp(KeyCode.E))
            {
                SpawnGroupThree();
            }
            else if (Input.GetKeyUp(KeyCode.R))
            {
                SpawnGrouping();
            }
        }
    }
}
