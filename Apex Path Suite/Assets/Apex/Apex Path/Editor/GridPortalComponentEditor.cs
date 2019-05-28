namespace Apex.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Apex.WorldGeometry;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(GridPortalComponent), true)]
    public class GridPortalComponentEditor : Editor
    {
        private static List<KeyValuePair<string, Type>> _actionsList;
        private static string[] _actionsNames;

        private SerializedProperty _portalName;
        private SerializedProperty _type;
        private SerializedProperty _direction;
        private SerializedProperty _exclusiveTo;
        private SerializedProperty _relativeToTransform;
        private SerializedProperty _portalOne;
        private SerializedProperty _portalTwo;
        private SerializedProperty _drawGizmosAlways;
        private SerializedProperty _portalOneColor;
        private SerializedProperty _portalTwoColor;
        private SerializedProperty _connectionColor;

        private int _idHash;
        private bool _inPortalMode;
        private Vector3 _portalRectStart;
        private Vector3 _portalRectEnd;
        private bool _shiftDown;
        private bool _gridConnectMode;

        public override void OnInspectorGUI()
        {
            if (Application.isPlaying)
            {
                this.serializedObject.Update();
                EditorGUILayout.PropertyField(_exclusiveTo);
                GUI.enabled = false;
                EditorGUILayout.PropertyField(_type);
                EditorGUILayout.PropertyField(_direction);
                GUI.enabled = true;
                return;
            }

            this.serializedObject.Update();
            var relativeCurrent = _relativeToTransform.boolValue;
            var typeCurrent = (PortalType)_type.intValue;

            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(_portalName);
            EditorGUILayout.PropertyField(_exclusiveTo);
            EditorGUILayout.PropertyField(_type);
            EditorGUILayout.PropertyField(_direction);

            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(_relativeToTransform);

            var p = this.target as GridPortalComponent;
            if (_relativeToTransform.boolValue != relativeCurrent)
            {
                if (_relativeToTransform.boolValue)
                {
                    var curVal = _portalOne.boundsValue;
                    curVal.center = p.transform.InverseTransformPoint(curVal.center);
                    _portalOne.boundsValue = curVal;

                    curVal = _portalTwo.boundsValue;
                    curVal.center = p.transform.InverseTransformPoint(curVal.center);
                    _portalTwo.boundsValue = curVal;
                }
                else
                {
                    var curVal = _portalOne.boundsValue;
                    curVal.center = p.transform.TransformPoint(curVal.center);
                    _portalOne.boundsValue = curVal;

                    curVal = _portalTwo.boundsValue;
                    curVal.center = p.transform.TransformPoint(curVal.center);
                    _portalTwo.boundsValue = curVal;
                }
            }

            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(_drawGizmosAlways);
            EditorGUILayout.PropertyField(_portalOneColor);
            EditorGUILayout.PropertyField(_portalTwoColor);
            EditorGUILayout.PropertyField(_connectionColor);
            this.serializedObject.ApplyModifiedProperties();

            //Make sure connector portals obey the rules. This must be done outside the serialized object stuff as we manipulate the portal directly.
            var typeNew = (PortalType)_type.intValue;
            if (typeNew != typeCurrent && typeNew == PortalType.Connector)
            {
                EnsureValidConnector(p);
            }

            ShowPortalCommands(p);

            if (typeNew != PortalType.Connector)
            {
                ShowActionSelector();
            }
        }

        private static void EnsureValidConnector(GridPortalComponent p)
        {
            var p1 = p.portalOne;
            var p2 = p.portalTwo;

            var p1WorldCenter = p1.center;
            var p2WorldCenter = p2.center;
            if (p.relativeToTransform)
            {
                var t = p.transform;
                p1WorldCenter = t.TransformPoint(p1WorldCenter);
                p2WorldCenter = t.TransformPoint(p2WorldCenter);
            }

            var g1 = GridManager.instance.GetGridComponent(p1WorldCenter);
            var g2 = GridManager.instance.GetGridComponent(p2WorldCenter);

            //If either portal has not yet been defined
            if (p1.size.sqrMagnitude == 0 && p2.size.sqrMagnitude == 0)
            {
                return;
            }
            else if (p1.size.sqrMagnitude == 0)
            {
                p.portalTwo = SizeAsConnector(p2, g2.cellSize);
                return;
            }
            else if (p2.size.sqrMagnitude == 0)
            {
                p.portalOne = SizeAsConnector(p1, g1.cellSize);
                return;
            }

            p1 = SizeAsConnector(p1, g1.cellSize);

            var rel = g2.origin - g1.origin;
            if (rel.x > rel.z)
            {
                p2 = new Bounds(new Vector3(p2.center.x, p2.center.y, p1.center.z), new Vector3(p1.size.x, p2.size.y, p1.size.z));
            }
            else
            {
                p2 = new Bounds(new Vector3(p1.center.x, p2.center.y, p2.center.z), new Vector3(p1.size.x, p2.size.y, p1.size.z));
            }

            p.portalOne = p1;
            p.portalTwo = p2;
        }

        private static Bounds SizeAsConnector(Bounds b, float cellSize)
        {
            var min = b.min;
            if (b.size.z > b.size.x)
            {
                min.x = b.max.x - (cellSize - 0.1f);
                b.SetMinMax(min, b.max);
            }
            else
            {
                min.z = b.max.z - (cellSize - 0.1f);
                b.SetMinMax(min, b.max);
            }

            return b;
        }

        private static void DrawActiveIndication(GridPortalComponent p)
        {
            Handles.color = Color.white;
            var radius = 0.5f + ((Mathf.Cos(Time.realtimeSinceStartup * 10f) + 1f) / 6f);
            var p1 = p.portalOne.center;
            var p2 = p.portalTwo.center;
            if (p.relativeToTransform)
            {
                var t = p.transform;
                p1 = t.TransformPoint(p1);
                p2 = t.TransformPoint(p2);
            }

            Handles.DrawWireDisc(p1, Vector3.up, radius);
            Handles.DrawWireDisc(p2, Vector3.up, radius);
        }

        private void OnEnable()
        {
            _portalName = this.serializedObject.FindProperty("portalName");
            _type = this.serializedObject.FindProperty("type");
            _direction = this.serializedObject.FindProperty("direction");
            _exclusiveTo = this.serializedObject.FindProperty("_exclusiveTo");
            _relativeToTransform = this.serializedObject.FindProperty("relativeToTransform");
            _portalOne = this.serializedObject.FindProperty("portalOne");
            _portalTwo = this.serializedObject.FindProperty("portalTwo");
            _drawGizmosAlways = this.serializedObject.FindProperty("drawGizmosAlways");
            _portalOneColor = this.serializedObject.FindProperty("portalOneColor");
            _portalTwoColor = this.serializedObject.FindProperty("portalTwoColor");
            _connectionColor = this.serializedObject.FindProperty("connectionColor");

            _idHash = this.GetHashCode();
        }

        private void ToggleEditMode(bool on)
        {
            if (on)
            {
                _inPortalMode = true;
                EditorApplication.update += OnUpdate;
            }
            else
            {
                _inPortalMode = false;
                EditorApplication.update -= OnUpdate;
                OnUpdate();
            }
        }

        private void ShowPortalCommands(GridPortalComponent p)
        {
            EditorGUILayout.Separator();
            if (_inPortalMode)
            {
                var msg = "Using the Left mouse button, click and drag to place Portal One.\n\nUsing the Left mouse button and Shift, click and drag to place Portal Two.\n\nHolding Control or Command with either option will expand the portal along the nearest perimeter.";
                if (p.type == PortalType.Connector)
                {
                    msg = string.Concat(msg, "\n\nTo easily place a connector portal between adjacent grids, simply drag the rectangle over the two grids to connect.");
                }

                EditorGUILayout.HelpBox(msg, MessageType.Info);
                EditorGUILayout.Separator();

                if (GUILayout.Button("Done (Esc)"))
                {
                    ToggleEditMode(false);
                }
            }
            else if (GUILayout.Button("Edit Portals"))
            {
                ToggleEditMode(true);

                if (SceneView.sceneViews.Count > 0)
                {
                    ((SceneView)SceneView.sceneViews[0]).Focus();
                }
            }
        }

        private void OnUpdate()
        {
            var sv = SceneView.lastActiveSceneView;
            if (sv != null)
            {
                sv.Repaint();
            }
        }

        private void ShowActionSelector()
        {
            var portal = this.target as GridPortalComponent;

            object pa = portal.As<IPortalAction>();
            if (pa == null)
            {
                pa = portal.As<IPortalActionFactory>();
            }

            if (pa == null)
            {
                if (_actionsList == null)
                {
                    _actionsList = new List<KeyValuePair<string, Type>>();

                    var asm = Assembly.GetAssembly(typeof(GridPortalComponent));
                    foreach (var actionType in asm.GetTypes().Where(t => (typeof(IPortalActionFactory).IsAssignableFrom(t) || typeof(IPortalAction).IsAssignableFrom(t)) && t.IsSubclassOf(typeof(MonoBehaviour)) && t.IsClass && !t.IsAbstract))
                    {
                        var actionName = actionType.Name;

                        var acm = Attribute.GetCustomAttribute(actionType, typeof(AddComponentMenu)) as AddComponentMenu;
                        if (acm != null)
                        {
                            var startIdx = acm.componentMenu.LastIndexOf('/') + 1;
                            actionName = acm.componentMenu.Substring(startIdx);
                        }

                        var pair = new KeyValuePair<string, Type>(actionName, actionType);
                        _actionsList.Add(pair);
                    }

                    _actionsList.Sort((a, b) => a.Key.CompareTo(b.Key));
                    _actionsNames = _actionsList.Select(p => p.Key).ToArray();
                }

                EditorGUILayout.Separator();
                var style = new GUIStyle(GUI.skin.label);
                style.normal.textColor = Color.yellow;
                EditorGUILayout.LabelField("Select a Portal Action", style);
                var selectedActionIdx = EditorGUILayout.Popup(-1, _actionsNames);
                if (selectedActionIdx >= 0)
                {
                    portal.gameObject.AddComponent(_actionsList[selectedActionIdx].Value);
                }
            }
        }

        private void HandlePortalDefinition()
        {
            if (!_inPortalMode)
            {
                return;
            }

            var p = this.target as GridPortalComponent;
            int id = GUIUtility.GetControlID(_idHash, FocusType.Passive);
            var groundRect = new Plane(Vector3.up, new Vector3(0f, 0f, 0f));

            var evt = Event.current;
            if (evt.type == EventType.MouseDown && evt.button == 0)
            {
                GUIUtility.hotControl = id;

                evt.Use();

                if (!EditorUtilitiesInternal.MouseToWorldPoint(groundRect, out _portalRectStart))
                {
                    GUIUtility.hotControl = 0;
                }

                _shiftDown = (evt.modifiers & EventModifiers.Shift) > 0;
                _gridConnectMode = (evt.modifiers & EventModifiers.Control) > 0 || (evt.modifiers & EventModifiers.Command) > 0;
                _portalRectEnd = _portalRectStart;
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
                ToggleEditMode(false);
                this.Repaint();

                return;
            }
            else if (GUIUtility.hotControl != id)
            {
                if (evt.type == EventType.Repaint)
                {
                    DrawActiveIndication(p);
                }

                return;
            }

            if (evt.type == EventType.MouseDrag)
            {
                evt.Use();

                if (!EditorUtilitiesInternal.MouseToWorldPoint(groundRect, out _portalRectEnd))
                {
                    _portalRectEnd = _portalRectStart;
                }
            }
            else if (evt.type == EventType.MouseUp)
            {
                GUIUtility.hotControl = 0;
                evt.Use();

                var startToEnd = (_portalRectEnd - _portalRectStart);
                var portalBounds = new Bounds(
                    _portalRectStart + (startToEnd * 0.5f),
                    new Vector3(Mathf.Abs(startToEnd.x), Mathf.Max(Mathf.Abs(startToEnd.y), 0.1f), Mathf.Abs(startToEnd.z)));

                _gridConnectMode = _gridConnectMode || (evt.modifiers & EventModifiers.Control) > 0 || (evt.modifiers & EventModifiers.Command) > 0;
                _shiftDown = _shiftDown || (evt.modifiers & EventModifiers.Shift) > 0;

                if (p.type == PortalType.Connector)
                {
                    HandleConnector(portalBounds, p);
                }
                else
                {
                    HandleNormal(portalBounds, p);
                }

                EditorUtility.SetDirty(p);
            }
            else if (evt.type == EventType.Repaint)
            {
                Handles.color = _shiftDown ? p.portalTwoColor : p.portalOneColor;
                var y = Mathf.Max(_portalRectStart.y, _portalRectEnd.y);
                var c1 = new Vector3(_portalRectStart.x, y, _portalRectEnd.z);
                var c2 = new Vector3(_portalRectEnd.x, y, _portalRectStart.z);
                Handles.DrawDottedLine(_portalRectStart, c1, 10f);
                Handles.DrawDottedLine(c1, _portalRectEnd, 10f);
                Handles.DrawDottedLine(_portalRectEnd, c2, 10f);
                Handles.DrawDottedLine(c2, _portalRectStart, 10f);
            }
        }

        private void HandleConnector(Bounds portalBounds, GridPortalComponent p)
        {
            var g1 = GridManager.instance.GetGridComponent(_portalRectStart);
            var g2 = GridManager.instance.GetGridComponent(_portalRectEnd);

            if (g1 != null && g2 != null && g1 != g2)
            {
                var p1 = EditorUtilitiesInternal.SnapToGridEdge(g1, portalBounds, _gridConnectMode);
                var p2 = EditorUtilitiesInternal.SnapToGridEdge(g2, portalBounds, _gridConnectMode);

                if (p.relativeToTransform)
                {
                    p1.center = p.transform.InverseTransformPoint(p1.center);
                    p2.center = p.transform.InverseTransformPoint(p2.center);
                }

                p.portalOne = p1;
                p.portalTwo = p2;
            }
            else
            {
                HandleNormal(portalBounds, p);
                EnsureValidConnector(p);
            }
        }

        private void HandleNormal(Bounds portalBounds, GridPortalComponent p)
        {
            var grid = GridManager.instance.GetGridComponent(_portalRectStart);
            if (grid == null)
            {
                grid = GridManager.instance.GetGridComponent(_portalRectEnd);
                if (grid == null)
                {
                    return;
                }
            }

            portalBounds = EditorUtilitiesInternal.SnapToGrid(grid, portalBounds, _gridConnectMode);

            if (p.relativeToTransform)
            {
                portalBounds.center = p.transform.InverseTransformPoint(portalBounds.center);
            }

            if (_shiftDown)
            {
                p.portalTwo = portalBounds;
            }
            else
            {
                p.portalOne = portalBounds;
            }
        }

        private void OnSceneGUI()
        {
            HandlePortalDefinition();
        }
    }
}
