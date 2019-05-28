#pragma warning disable 1591
namespace Apex.Examples.SceneSpecific.ChangeUnitSpeed
{
    using System.Collections.Generic;
    using Apex.GameState;
    using Apex.Services;
    using Apex.Steering;
    using Apex.Units;
    using UnityEngine;

    [AddComponentMenu("Apex/Examples/SceneSpecific/ChangeUnitSpeed/Unit Handler", 1001)]
    public class UnitHandler : MonoBehaviour
    {
        private TransientGroup<IUnitFacade> _aiGroup;
        private string _speedValue = "10";

        public GameObject unitMold;
        public GameObject soloUnit;

        private void Start()
        {
            //Create the AI group. In this case we just explicitly create one group
            //but the group strategy could also be used to create a grouping
            _aiGroup = new DefaultTransientUnitGroup(4);
            var aiStartPos = new Vector3(-20f, 0f, 0f);
            for (int i = 0; i < 4; i++)
            {
                var unitGO = Instantiate(this.unitMold, aiStartPos, Quaternion.identity) as GameObject;
                unitGO.SetActive(true);
                _aiGroup.Add(unitGO.GetUnitFacade());
            }
        }

        private void OnGUI()
        {
            //Yes don't put in invalid values, this code is clearly fragile
            _speedValue = GUI.TextField(new Rect(10, 10, 70, 20), _speedValue, 2);
            var targetSpeed = int.Parse(_speedValue);

            if (GUI.Button(new Rect(90, 10, 140, 50), "Change AI Speed"))
            {
                _aiGroup.SetPreferredSpeed(targetSpeed);
            }

            if (GUI.Button(new Rect(240, 10, 150, 50), "Change Selected Speed"))
            {
                var selectedUnits = GameServices.gameStateManager.unitSelection.selected;

                selectedUnits.SetPreferredSpeed(targetSpeed);
            }

            if (GUI.Button(new Rect(400, 10, 140, 50), "Change Solo Speed"))
            {
                var unit = this.soloUnit.GetUnitFacade();

                unit.SetPreferredSpeed(targetSpeed);
            }

            if (GUI.Button(new Rect(550, 10, 140, 50), "Change For All"))
            {
                ChangeSpeedForAll(targetSpeed);
            }
        }

        private void ChangeSpeedForAll(float speed)
        {
            var allUnits = GameServices.gameStateManager.units;

            //Now we could just set the speed for each individual unit, this would work just fine in Apex Path
            //However other packages add additional virtual units to groups for various reasons, so the most robust way
            //is to change the speed on the group if one exists.
            var processedGroups = new HashSet<object>();
            foreach (var unit in allUnits)
            {
                var grp = unit.transientGroup;

                if (grp == null)
                {
                    unit.SetPreferredSpeed(speed);
                }
                else if (processedGroups.Add(grp))
                {
                    grp.SetPreferredSpeed(speed);
                }
            }
        }
    }
}
