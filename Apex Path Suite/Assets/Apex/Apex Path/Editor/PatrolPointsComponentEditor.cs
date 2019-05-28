/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Editor
{
    using System;
    using Apex.Steering.Behaviours;
    using UnityEditor;
    using UnityEngine;
    using WorldGeometry;

    [CustomEditor(typeof(PatrolPointsComponent), false)]
    public class PatrolPointsComponentEditor : Editor
    {
        private static readonly KeyCode _addKey = KeyCode.A;
        private static readonly KeyCode _removeKey = KeyCode.D;

        private SerializedProperty _points;
        private SerializedProperty _relativeToTransform;
        private SerializedProperty _pointColor;
        private SerializedProperty _textColor;

        private int _id;
        private bool _inPlacementMode;
        private bool _emphasize;
        private int _movingIdx;
        private GUIStyle _numberStyle;

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(_pointColor);
            EditorGUILayout.PropertyField(_textColor);
            EditorGUILayout.Separator();

            var relativeCurrent = _relativeToTransform.boolValue;
            EditorGUILayout.PropertyField(_relativeToTransform);
            EditorGUILayout.PropertyField(_points, true);

            this.serializedObject.ApplyModifiedProperties();

            if (_relativeToTransform.boolValue != relativeCurrent)
            {
                var p = this.target as PatrolPointsComponent;
                var points = p.points;
                var t = p.transform;

                if (_relativeToTransform.boolValue)
                {
                    for (int i = 0; i < points.Length; i++)
                    {
                        points[i] = t.InverseTransformPoint(points[i]);
                    }
                }
                else
                {
                    for (int i = 0; i < points.Length; i++)
                    {
                        points[i] = t.TransformPoint(points[i]);
                    }
                }

                EditorUtility.SetDirty(p);
            }

            EditorGUILayout.Separator();
            if (_inPlacementMode)
            {
                var msg = string.Format("Use the {0} key to place points at the mouse cursor.\nUse the {1} key to remove points at the mouse cursor.\n\nClick and drag a point to move it.\nHold Control to switch to standard mouse mode.", _addKey, _removeKey);
                EditorGUILayout.HelpBox(msg, MessageType.Info);
                EditorGUILayout.Separator();

                if (GUILayout.Button("Done (Esc)"))
                {
                    _inPlacementMode = false;
                }
            }
            else if (GUILayout.Button("Edit Points"))
            {
                _inPlacementMode = true;

                if (SceneView.sceneViews.Count > 0)
                {
                    ((SceneView)SceneView.sceneViews[0]).Focus();
                }
            }

            var newEmphasize = GUILayout.Toggle(_emphasize, "Emphasize Points", GUI.skin.button);
            if (newEmphasize != _emphasize)
            {
                _emphasize = newEmphasize;
                if (_emphasize)
                {
                    EditorApplication.update += OnUpdate;
                }
                else
                {
                    EditorApplication.update -= OnUpdate;
                    OnUpdate();
                }
            }
        }

        private static void DrawActiveIndication(PatrolPointsComponent p)
        {
            Handles.color = Color.white;
            var radius = 0.5f + ((Mathf.Cos(Time.realtimeSinceStartup * 10f) + 1f) / 6f);

            var points = p.worldPoints;
            for (int i = 0; i < points.Length; i++)
            {
                Handles.DrawWireDisc(points[i], Vector3.up, radius);
            }
        }

        private void DrawNumberLabels(PatrolPointsComponent p)
        {
            if (_numberStyle == null)
            {
                _numberStyle = new GUIStyle(GUI.skin.label);
                _numberStyle.fontStyle = FontStyle.Bold;
            }

            _numberStyle.normal.textColor = p.textColor;
            var points = p.worldPoints;
            for (int i = 0; i < points.Length; i++)
            {
                var pos = points[i];
                pos.x -= 0.1f;
                pos.y += 1f;
                pos.z += 0.3f;
                Handles.Label(pos, i.ToString(), _numberStyle);
            }
        }

        private void OnEnable()
        {
            _points = this.serializedObject.FindProperty("points");
            _relativeToTransform = this.serializedObject.FindProperty("relativeToTransform");
            _pointColor = this.serializedObject.FindProperty("pointColor");
            _textColor = this.serializedObject.FindProperty("textColor");

            _id = GUIUtility.GetControlID(this.GetHashCode(), FocusType.Passive);
        }

        private void OnUpdate()
        {
            var sv = SceneView.lastActiveSceneView;
            if (sv != null)
            {
                sv.Repaint();
            }
        }

        private void OnSceneGUI()
        {
            var evt = Event.current;
            var p = this.target as PatrolPointsComponent;

            if (evt.type == EventType.Repaint)
            {
                if (_emphasize && GUIUtility.hotControl != _id)
                {
                    DrawActiveIndication(p);
                }

                DrawNumberLabels(p);
                return;
            }

            if (!_inPlacementMode)
            {
                return;
            }

            var points = p.points;
            var groundRect = new Plane(Vector3.up, Vector3.zero);

            if (evt.type == EventType.KeyDown && evt.keyCode == _addKey)
            {
                GUIUtility.hotControl = _id;

                evt.Use();
            }
            else if (evt.type == EventType.KeyUp && evt.keyCode == _addKey)
            {
                evt.Use();

                Vector3 point;
                if (!EditorUtilitiesInternal.MouseToWorldPoint(groundRect, out point))
                {
                    GUIUtility.hotControl = 0;
                    return;
                }

                if (p.relativeToTransform)
                {
                    point = p.transform.InverseTransformPoint(point);
                }

                int idx = points.Length;

                var tmp = new Vector3[idx + 1];
                Array.Copy(points, tmp, points.Length);
                points = tmp;
                p.points = tmp;

                points[idx] = point;
                EditorUtility.SetDirty(p);
                GUIUtility.hotControl = 0;
            }
            else if (evt.type == EventType.KeyDown && evt.keyCode == _removeKey)
            {
                GUIUtility.hotControl = _id;

                evt.Use();
            }
            else if (evt.type == EventType.KeyUp && evt.keyCode == _removeKey)
            {
                evt.Use();

                Vector3 point;
                if (!EditorUtilitiesInternal.MouseToWorldPoint(groundRect, out point))
                {
                    GUIUtility.hotControl = 0;
                    return;
                }

                var removeIdx = -1;
                var lineOrigin = Camera.current.transform.position;

                for (int i = 0; i < points.Length; i++)
                {
                    //If Gizmos change so should this
                    var sphereCenter = points[i] + Vector3.up;
                    if (Geometry.DoesLineIntersectSphere(lineOrigin, point, sphereCenter, 0.3f))
                    {
                        removeIdx = i;
                        break;
                    }
                }

                if (removeIdx >= 0)
                {
                    var tmp = new Vector3[points.Length - 1];
                    Array.Copy(points, tmp, removeIdx);
                    Array.Copy(points, removeIdx + 1, tmp, removeIdx, points.Length - (removeIdx + 1));
                    points = tmp;
                    p.points = tmp;

                    EditorUtility.SetDirty(p);
                }

                GUIUtility.hotControl = 0;
            }
            else if (evt.type == EventType.KeyDown && evt.keyCode == KeyCode.Escape)
            {
                GUIUtility.hotControl = _id;
                evt.Use();
            }
            else if (evt.type == EventType.KeyUp && evt.keyCode == KeyCode.Escape)
            {
                GUIUtility.hotControl = 0;
                evt.Use();
                _inPlacementMode = false;
                this.Repaint();
            }
            else if (evt.control)
            {
                return;
            }
            else if (evt.type == EventType.MouseDown && evt.button == MouseButton.left)
            {
                GUIUtility.hotControl = _id;
                evt.Use();
                _movingIdx = -1;

                Vector3 point;
                if (!EditorUtilitiesInternal.MouseToWorldPoint(groundRect, out point))
                {
                    return;
                }

                var lineOrigin = Camera.current.transform.position;

                for (int i = 0; i < points.Length; i++)
                {
                    //If Gizmos change so should this
                    var sphereCenter = points[i] + Vector3.up;
                    if (Geometry.DoesLineIntersectSphere(lineOrigin, point, sphereCenter, 0.3f))
                    {
                        _movingIdx = i;
                        break;
                    }
                }
            }
            else if (evt.type == EventType.MouseUp && evt.button == MouseButton.left)
            {
                GUIUtility.hotControl = 0;
                evt.Use();

                if (_movingIdx >= 0)
                {
                    EditorUtility.SetDirty(p);
                    _movingIdx = -1;
                }
            }
            else if (evt.type == EventType.MouseDrag && evt.button == MouseButton.left && _movingIdx >= 0)
            {
                evt.Use();

                Vector3 point;
                if (!EditorUtilitiesInternal.MouseToWorldPoint(groundRect, out point))
                {
                    return;
                }

                points[_movingIdx] = point;
            }
        }
    }
}
