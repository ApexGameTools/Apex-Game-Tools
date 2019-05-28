namespace Apex.Editor
{
    using Apex.Debugging;
    using Apex.Services;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(GridVisualizer), false), CanEditMultipleObjects]
    public class GridVisualizerEditor : Editor
    {
        private SerializedProperty _drawMode;
        private SerializedProperty _modelHeightNavigationCapabilities;
        private SerializedProperty _modelAttributes;
        private SerializedProperty _drawSubSections;
        private SerializedProperty _drawAllGrids;
        private SerializedProperty _editorRefreshDelay;
        private SerializedProperty _drawDistanceThreshold;
        private SerializedProperty _gridLinesColor;
        private SerializedProperty _descentOnlyColor;
        private SerializedProperty _ascentOnlyColor;
        private SerializedProperty _obstacleColor;
        private SerializedProperty _subSectionsColor;
        private SerializedProperty _boundsColor;
        private SerializedProperty _drawAlways;

        private bool _heightStrategyMissing;

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();
            EditorGUILayout.Separator();

            EditorGUILayout.PropertyField(_drawMode);
            if (_drawMode.intValue == (int)GridVisualizer.GridMode.Accessibility)
            {
                if (_heightStrategyMissing || !GameServices.heightStrategy.useGlobalHeightNavigationSettings)
                {
                    EditorGUILayout.PropertyField(_modelHeightNavigationCapabilities, new GUIContent("Model Settings", "Change these to see how the grid looks to units with these settings."));
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(_modelAttributes);
                    EditorGUI.indentLevel--;
                }

                if (!Application.isPlaying)
                {
                    EditorGUILayout.HelpBox("Please note that in order to show height map data, the grid(s) must be baked.", MessageType.Info);
                }
            }

            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(_drawAlways);
            EditorGUILayout.PropertyField(_drawAllGrids);
            EditorGUILayout.PropertyField(_drawSubSections);
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(_editorRefreshDelay);
            EditorGUILayout.PropertyField(_drawDistanceThreshold);
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(_gridLinesColor);
            EditorGUILayout.PropertyField(_ascentOnlyColor);
            EditorGUILayout.PropertyField(_descentOnlyColor);
            EditorGUILayout.PropertyField(_obstacleColor);
            EditorGUILayout.PropertyField(_subSectionsColor);
            EditorGUILayout.PropertyField(_boundsColor);

            this.serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            _drawMode = this.serializedObject.FindProperty("drawMode");
            _modelHeightNavigationCapabilities = this.serializedObject.FindProperty("modelHeightNavigationCapabilities");
            _modelAttributes = this.serializedObject.FindProperty("modelAttributes");
            _drawSubSections = this.serializedObject.FindProperty("drawSubSections");
            _drawAllGrids = this.serializedObject.FindProperty("drawAllGrids");
            _editorRefreshDelay = this.serializedObject.FindProperty("editorRefreshDelay");
            _drawDistanceThreshold = this.serializedObject.FindProperty("drawDistanceThreshold");
            _gridLinesColor = this.serializedObject.FindProperty("gridLinesColor");
            _descentOnlyColor = this.serializedObject.FindProperty("descentOnlyColor");
            _ascentOnlyColor = this.serializedObject.FindProperty("ascentOnlyColor");
            _obstacleColor = this.serializedObject.FindProperty("obstacleColor");
            _subSectionsColor = this.serializedObject.FindProperty("subSectionsColor");
            _boundsColor = this.serializedObject.FindProperty("boundsColor");
            _drawAlways = this.serializedObject.FindProperty("drawAlways");

            _heightStrategyMissing = (GameServices.heightStrategy == null);
        }
    }
}
