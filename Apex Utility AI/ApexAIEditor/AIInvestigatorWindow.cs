/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Editor
{
    using UnityEditor;
    using UnityEngine;

    public class AIInvestigatorWindow : EditorWindow
    {
        private static readonly string[] _headerTabTitles = new string[] { "Referenced AIs", "Referenced Types" };
        private Tool _tool;
        private AIInvestigator _aiInvestigator;
        private TypeInvestigator _typeInvestigator;

        private enum Tool
        {
            ReferencedAIs,
            ReferencedTypes
        }

        public static void ShowWindow()
        {
            EditorWindow.GetWindow<AIInvestigatorWindow>(true, "Apex AI Investigator");
        }

        private void OnEnable()
        {
            this.minSize = new Vector2(550f, 495f);
            _aiInvestigator = new AIInvestigator(this);
            _typeInvestigator = new TypeInvestigator(this);
        }

        private void OnDisable()
        {
            _aiInvestigator.Reset();
            _typeInvestigator.Reset();
        }

        private void OnGUI()
        {
            EditorStyling.InitScaleAgnosticStyles();

            EditorGUILayout.BeginHorizontal();
            _tool = (Tool)GUILayout.SelectionGrid((int)_tool, _headerTabTitles, 2, EditorStyles.toolbarButton);
            EditorGUILayout.EndHorizontal();

            if (_tool == Tool.ReferencedAIs)
            {
                _aiInvestigator.Render();
            }
            else
            {
                _typeInvestigator.Render();
            }
        }
    }
}
