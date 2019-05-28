/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Editor
{
    using Apex.AI.Visualization;
    using Apex.Editor;
    using UnityEditor;
    using UnityEngine;

    public sealed class BreakpointConditionWindow : EditorWindow
    {
        private static readonly string[] _operators = new string[] { "<", "<=", "==", "!=", ">=", ">" };
        private BreakpointCondition _condition;
        private IQualifierVisualizer _qv;

        internal static BreakpointConditionWindow Open(Vector2 screenPosition, IQualifierVisualizer qv)
        {
            var host = EditorWindow.focusedWindow;
            var win = EditorWindow.GetWindow<BreakpointConditionWindow>(true, "Breakpoint Condition", false);
            win.Init(screenPosition, qv, host);
            win.Show();
            win.Focus();
            return win;
        }

        private void Init(Vector2 screenPosition, IQualifierVisualizer qv, EditorWindow host)
        {
            this.minSize = new Vector2(210f, 50f);
            this.maxSize = new Vector2(230f, 80f);

            var winRect = this.position;
            winRect.size = new Vector2(220f, 60f);
            this.position = PopupConstraints.GetValidPosition(winRect, screenPosition, host);

            _qv = qv;
            _condition = new BreakpointCondition();
            var current = qv.breakpointCondition;
            if (current != null)
            {
                _condition.compareOperator = current.compareOperator;
                _condition.scoreThreshold = current.scoreThreshold;
            }
        }

        private void OnLostFocus()
        {
            //Closing directly here causes an exception for some reason, however delaying it a frame solves it
            SafeClose();
        }

        private void SafeClose()
        {
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnEditorUpdate()
        {
            EditorApplication.update -= OnEditorUpdate;
            this.Close();
        }

        private void OnGUI()
        {
            EditorGUILayout.Separator();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Score ", GUILayout.Width(60f));
            _condition.compareOperator = (CompareOperator)(EditorGUILayout.Popup((int)_condition.compareOperator - 1, _operators) + 1);
            _condition.scoreThreshold = EditorGUILayout.FloatField(_condition.scoreThreshold);
            EditorGUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Ok"))
            {
                _qv.breakpointCondition = _condition;
                _qv.isBreakPoint = true;
                AIEditorWindow.activeInstance.Repaint();
                this.Close();
            }

            if (GUILayout.Button("Cancel"))
            {
                this.Close();
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}
