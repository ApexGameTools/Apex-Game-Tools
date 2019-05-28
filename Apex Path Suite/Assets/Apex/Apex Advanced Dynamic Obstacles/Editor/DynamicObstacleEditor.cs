/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Editor
{
    using Apex.WorldGeometry;
    using UnityEditor;
    using UnityEngine;

    public partial class DynamicObstacleEditor : Editor
    {
        private SerializedProperty _boundingMode;
        private SerializedProperty _adaptationRotationThreshold;

        partial void ExtensionOnGUI()
        {
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(_boundingMode, new GUIContent("Bounding Mode", "Controls how the dynamic obstacle determines which cells it covers."));
            if (_boundingMode.intValue == (int)DynamicObstacle.Bounding.Adaptive)
            {
                EditorGUILayout.PropertyField(_adaptationRotationThreshold, new GUIContent("Adaptation Rotation Threshold", "Controls how much rotation around the x- and/or z-axis is required before switching to full generic mode, which is more precise but also more costly."));
            }
        }

        partial void ExtensionEnable()
        {
            _boundingMode = this.serializedObject.FindProperty("boundingMode");
            _adaptationRotationThreshold = this.serializedObject.FindProperty("adaptationRotationThreshold");
        }
    }
}
