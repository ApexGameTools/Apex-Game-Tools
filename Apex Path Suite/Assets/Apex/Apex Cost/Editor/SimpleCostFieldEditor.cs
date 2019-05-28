/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Editor
{
    using Apex.PathFinding;
    using Apex.WorldGeometry;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(SimpleCostField), false), CanEditMultipleObjects]
    public class SimpleCostFieldEditor : Editor
    {
        private SerializedProperty _cost;
        private SerializedProperty _bounds;
        private SerializedProperty _relativeToTransform;
        private SerializedProperty _gizmoColor;

        private int _idHash;
        private bool _inDrawMode;
        private Vector3 _boundsRectStart;
        private Vector3 _boundsRectEnd;

        public override void OnInspectorGUI()
        {
            GUI.enabled = !EditorApplication.isPlaying;

            this.serializedObject.Update();
            var relativeCurrent = _relativeToTransform.boolValue;

            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(_relativeToTransform, new GUIContent("Relative To Transform", "Controls whether the portal end points are seen as relative to their parent transform. IF relative they will move with the transform otherwise they will be static and remain where they were placed initially."));
            if (_relativeToTransform.boolValue != relativeCurrent)
            {
                var p = this.target as SimpleCostField;
                if (_relativeToTransform.boolValue)
                {
                    var curVal = _bounds.boundsValue;
                    curVal.center = p.transform.InverseTransformPoint(curVal.center);
                    _bounds.boundsValue = curVal;
                }
                else
                {
                    var curVal = _bounds.boundsValue;
                    curVal.center = p.transform.TransformPoint(curVal.center);
                    _bounds.boundsValue = curVal;
                }
            }

            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(_cost);

            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(_gizmoColor);

            this.serializedObject.ApplyModifiedProperties();

            ShowDrawCommands();
            GUI.enabled = true;
        }

        private void OnEnable()
        {
            _cost = this.serializedObject.FindProperty("cost");
            _bounds = this.serializedObject.FindProperty("bounds");
            _relativeToTransform = this.serializedObject.FindProperty("relativeToTransform");
            _gizmoColor = this.serializedObject.FindProperty("gizmoColor");

            _idHash = this.GetHashCode();
        }

        private void ShowDrawCommands()
        {
            EditorGUILayout.Separator();
            if (_inDrawMode)
            {
                EditorGUILayout.HelpBox("Using the Left mouse button, click and drag to place the cost field.", MessageType.Info);
                EditorGUILayout.Separator();

                if (GUILayout.Button("Done (Esc)"))
                {
                    _inDrawMode = false;
                }
            }
            else if (GUILayout.Button("Edit Field"))
            {
                _inDrawMode = true;
                if (SceneView.sceneViews.Count > 0)
                {
                    ((SceneView)SceneView.sceneViews[0]).Focus();
                }
            }
        }

        private void HandleFieldDefinition()
        {
            if (!_inDrawMode)
            {
                return;
            }

            var p = this.target as SimpleCostField;
            int id = GUIUtility.GetControlID(_idHash, FocusType.Passive);
            var groundRect = new Plane(Vector3.up, new Vector3(0f, 0f, 0f));

            var evt = Event.current;
            if (evt.type == EventType.MouseDown && evt.button == 0)
            {
                GUIUtility.hotControl = id;

                evt.Use();

                if (!EditorUtilitiesInternal.MouseToWorldPoint(groundRect, out _boundsRectStart))
                {
                    GUIUtility.hotControl = 0;
                }

                _boundsRectEnd = _boundsRectStart;
                return;
            }
            else if (evt.type == EventType.KeyDown && evt.keyCode == KeyCode.Escape)
            {
                GUIUtility.hotControl = id;
                evt.Use();
                return;
            }
            else if (evt.type == EventType.KeyUp && evt.keyCode == KeyCode.Escape)
            {
                GUIUtility.hotControl = 0;
                evt.Use();
                _inDrawMode = false;
                this.Repaint();

                return;
            }
            else if (GUIUtility.hotControl != id)
            {
                return;
            }

            if (evt.type == EventType.MouseDrag)
            {
                evt.Use();

                if (!EditorUtilitiesInternal.MouseToWorldPoint(groundRect, out _boundsRectEnd))
                {
                    _boundsRectEnd = _boundsRectStart;
                }
            }
            else if (evt.type == EventType.MouseUp)
            {
                GUIUtility.hotControl = 0;
                evt.Use();

                _boundsRectStart.y = _boundsRectEnd.y = Mathf.Max(_boundsRectStart.y, _boundsRectEnd.y);

                var grid = GridManager.instance.GetGridComponent(_boundsRectStart);
                if (grid == null)
                {
                    grid = GridManager.instance.GetGridComponent(_boundsRectEnd);
                    if (grid == null)
                    {
                        return;
                    }
                }

                var startToEnd = (_boundsRectEnd - _boundsRectStart);
                var fieldBounds = new Bounds(
                    _boundsRectStart + (startToEnd * 0.5f),
                    new Vector3(Mathf.Abs(startToEnd.x), Mathf.Abs(startToEnd.y) + 0.1f, Mathf.Abs(startToEnd.z)));

                fieldBounds = EditorUtilitiesInternal.SnapToGrid(grid, fieldBounds, false);

                if (p.relativeToTransform)
                {
                    fieldBounds.center = p.transform.InverseTransformPoint(fieldBounds.center);
                }

                p.bounds = fieldBounds;

                EditorUtility.SetDirty(p);
            }
            else if (evt.type == EventType.Repaint)
            {
                Handles.color = p.gizmoColor;
                var c1 = new Vector3(_boundsRectStart.x, _boundsRectStart.y, _boundsRectEnd.z);
                var c2 = new Vector3(_boundsRectEnd.x, _boundsRectEnd.y, _boundsRectStart.z);
                Handles.DrawDottedLine(_boundsRectStart, c1, 10f);
                Handles.DrawDottedLine(c1, _boundsRectEnd, 10f);
                Handles.DrawDottedLine(_boundsRectEnd, c2, 10f);
                Handles.DrawDottedLine(c2, _boundsRectStart, 10f);
            }
        }

        private void OnSceneGUI()
        {
            HandleFieldDefinition();
        }
    }
}
