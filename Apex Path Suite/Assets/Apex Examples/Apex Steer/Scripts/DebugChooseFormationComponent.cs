/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591

namespace Apex.Examples.Steering
{
    using Apex.Services;
    using Apex.Steering;
    using UnityEngine;

    [AddComponentMenu("Apex/Examples/Steering/Formation Chooser GUI", 1010)]
    public class DebugChooseFormationComponent : ExtendedMonoBehaviour
    {
        public float formationSpacing = 1.5f;

        public GUISkin guiSkin;

        private void SetFormation(IFormation formation)
        {
            var selected = GameServices.gameStateManager.unitSelection.selected;
            if (formation != null)
            {
                selected.SetFormation(formation);
            }
            else
            {
                selected.ClearFormation();
            }
        }

        private void OnGUI()
        {
            var selected = GameServices.gameStateManager.unitSelection.selected;
            if (selected.groupCount <= 0 || selected.memberCount <= 0)
            {
                return;
            }

            if (guiSkin != null && GUI.skin != guiSkin)
            {
                GUI.skin = guiSkin;
            }

            float width = 150f;
            float height = Screen.height * 0.9f;
            float buttonHeight = height / 8f;

            GUILayout.BeginArea(new Rect(5f, (Screen.height / 2f) - (height / 2f), width, height));
            GUILayout.BeginVertical();

            if (GUILayout.Button("Circle Formation (F1)", GUILayout.Width(width), GUILayout.Height(buttonHeight)) || Input.GetKeyUp(KeyCode.F1))
            {
                SetFormation(new FormationEllipsoid(formationSpacing));
            }
            else if (GUILayout.Button("Grid Formation (F2)", GUILayout.Width(width), GUILayout.Height(buttonHeight)) || Input.GetKeyUp(KeyCode.F2))
            {
                SetFormation(new FormationGrid(formationSpacing));
            }
            else if (GUILayout.Button("Spiral Formation (F3)", GUILayout.Width(width), GUILayout.Height(buttonHeight)) || Input.GetKeyUp(KeyCode.F3))
            {
                SetFormation(new FormationSpiral(formationSpacing));
            }
            else if (GUILayout.Button("Wing Formation (F4)", GUILayout.Width(width), GUILayout.Height(buttonHeight)) || Input.GetKeyUp(KeyCode.F4))
            {
                SetFormation(new FormationWing(formationSpacing));
            }
            else if (GUILayout.Button("Row Formation (F5)", GUILayout.Width(width), GUILayout.Height(buttonHeight)) || Input.GetKeyUp(KeyCode.F5))
            {
                SetFormation(new FormationRow(formationSpacing));
            }
            else if (GUILayout.Button("Line Formation (F6)", GUILayout.Width(width), GUILayout.Height(buttonHeight)) || Input.GetKeyUp(KeyCode.F6))
            {
                SetFormation(new FormationLine(formationSpacing));
            }
            else if (GUILayout.Button("No Formation (F7)", GUILayout.Width(width), GUILayout.Height(buttonHeight)) || Input.GetKey(KeyCode.F7))
            {
                SetFormation(null);
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}