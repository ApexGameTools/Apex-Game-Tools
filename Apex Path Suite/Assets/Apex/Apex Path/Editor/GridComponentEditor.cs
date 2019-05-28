namespace Apex.Editor
{
    using Apex.DataStructures;
    using Apex.Services;
    using Apex.Steering;
    using Apex.WorldGeometry;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(GridComponent), true), CanEditMultipleObjects]
    public class GridComponentEditor : Editor
    {
        private SerializedProperty _friendlyName;
        private SerializedProperty _linkOriginToTransform;
        private SerializedProperty _origin;
        private SerializedProperty _originOffset;
        private SerializedProperty _sizeX;
        private SerializedProperty _sizeZ;
        private SerializedProperty _cellSize;
        private SerializedProperty _obstacleSensitivityRange;
        private SerializedProperty _subSectionsX;
        private SerializedProperty _subSectionsZ;
        private SerializedProperty _subSectionsCellOverlap;
        private SerializedProperty _generateHeightMap;
        private SerializedProperty _heightLookupType;
        private SerializedProperty _heightLookupMaxDepth;
        private SerializedProperty _lowerBoundary;
        private SerializedProperty _upperBoundary;
        private SerializedProperty _storeBakedDataAsAsset;
        private SerializedProperty _automaticInitialization;
        private SerializedProperty _automaticConnections;
        private SerializedProperty _obstacleAndGroundDetection;
        private SerializedProperty _connectorPortalWidth;

        private bool _heightStrategyMissing;

        public override void OnInspectorGUI()
        {
            GUI.enabled = !EditorApplication.isPlaying;

            this.serializedObject.Update();

            int baked = 0;
            var editedObjects = this.serializedObject.targetObjects;
            for (int i = 0; i < editedObjects.Length; i++)
            {
                var g = editedObjects[i] as GridComponent;
                if (g.bakedData != null)
                {
                    baked++;
                }
            }

            EditorGUILayout.Separator();

            if (baked > 0 && baked < editedObjects.Length)
            {
                EditorGUILayout.LabelField("A mix of baked and unbaked grids cannot be edited at the same time.");
                return;
            }

            //If data is baked, only offer an option to edit or rebake
            if (baked == editedObjects.Length)
            {
                EditorGUILayout.LabelField("The grid has been baked. To change it press the Edit button below.");

                GUILayout.BeginHorizontal();

                if (GUILayout.Button("Edit"))
                {
                    foreach (var o in editedObjects)
                    {
                        var g = o as GridComponent;
                        EditorUtilitiesInternal.RemoveAsset(g.bakedData);
                        g.bakedData = null;
                        g.ResetGrid();
                        EditorUtility.SetDirty(g);
                    }
                }

                if (GUILayout.Button("Re-bake Grid"))
                {
                    foreach (var o in editedObjects)
                    {
                        var g = o as GridComponent;
                        BakeGrid(g);
                    }
                }

                GUILayout.EndHorizontal();
                return;
            }

            EditorGUILayout.PropertyField(_friendlyName);

            EditorUtilities.Section("Layout");

            EditorGUILayout.PropertyField(_linkOriginToTransform);

            if (!_linkOriginToTransform.hasMultipleDifferentValues)
            {
                if (_linkOriginToTransform.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(_originOffset, true);
                    EditorGUI.indentLevel--;
                }
                else
                {
                    EditorGUILayout.PropertyField(_origin, true);
                }
            }

            EditorGUILayout.PropertyField(_sizeX);
            EditorGUILayout.PropertyField(_sizeZ);
            EditorGUILayout.PropertyField(_cellSize);
            EditorGUILayout.PropertyField(_lowerBoundary);
            EditorGUILayout.PropertyField(_upperBoundary);
            EditorGUILayout.PropertyField(_obstacleSensitivityRange);
            EditorGUILayout.PropertyField(_obstacleAndGroundDetection);

            EditorUtilities.Section("Subsections");

            EditorGUILayout.PropertyField(_subSectionsX);
            EditorGUILayout.PropertyField(_subSectionsZ);
            EditorGUILayout.PropertyField(_subSectionsCellOverlap);

            ShowHeightMapOptions();

            EditorUtilities.Section("Initialization");
            EditorGUILayout.PropertyField(_automaticInitialization);
            EditorGUILayout.PropertyField(_automaticConnections);
            if (_automaticConnections.boolValue)
            {
                EditorGUILayout.PropertyField(_connectorPortalWidth);
            }

            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(_storeBakedDataAsAsset);

            this.serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button(new GUIContent("Bake Grid", "Calculates grid data such as blocked areas and height map and stores this snapshot. The snapshot is then used to initialize the grid at runtime.\nPlease note that baking is completely optional.")))
            {
                foreach (var o in editedObjects)
                {
                    var g = o as GridComponent;
                    BakeGrid(g);
                }
            }

            GUI.enabled = true;
        }

        private static void BakeGrid(GridComponent g)
        {
            var builder = g.GetBuilder();

            var matrix = CellMatrix.Create(builder);

            var data = g.bakedData;
            if (data == null)
            {
                data = CellMatrixData.Create(matrix);

                g.bakedData = data;
            }
            else
            {
                data.Refresh(matrix);
            }

            if (g.storeBakedDataAsAsset)
            {
                EditorUtilitiesInternal.CreateOrUpdateAsset(data, g.friendlyName.Trim());
            }
            else
            {
                EditorUtility.SetDirty(data);
            }

            g.ResetGrid();
            EditorUtility.SetDirty(g);

            Debug.Log(string.Format("The grid {0} was successfully baked.", g.friendlyName));
        }

        private void ShowHeightMapOptions()
        {
            if (_heightStrategyMissing || GameServices.heightStrategy.heightMode == HeightSamplingMode.HeightMap)
            {
                EditorGUI.indentLevel--;
                EditorGUILayout.Separator();

                EditorGUILayout.PropertyField(_generateHeightMap);
                if (_generateHeightMap.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(_heightLookupType);
                    if (_heightLookupType.intValue == (int)HeightLookupType.QuadTree)
                    {
                        EditorGUILayout.PropertyField(_heightLookupMaxDepth);
                    }

                    EditorGUI.indentLevel--;
                }
            }
            else
            {
                EditorUtilities.Section("Height Map");
                EditorGUILayout.HelpBox(string.Format("Height navigation mode has been set to {0} (on Game World), hence no height map will be created.", GameServices.heightStrategy.heightMode), MessageType.Info);
            }
        }

        private void OnEnable()
        {
            _friendlyName = this.serializedObject.FindProperty("_friendlyName");

            _linkOriginToTransform = this.serializedObject.FindProperty("_linkOriginToTransform");
            _origin = this.serializedObject.FindProperty("_origin");
            _originOffset = this.serializedObject.FindProperty("_originOffset");
            _sizeX = this.serializedObject.FindProperty("sizeX");
            _sizeZ = this.serializedObject.FindProperty("sizeZ");
            _cellSize = this.serializedObject.FindProperty("cellSize");
            _obstacleSensitivityRange = this.serializedObject.FindProperty("obstacleSensitivityRange");

            _subSectionsX = this.serializedObject.FindProperty("subSectionsX");
            _subSectionsZ = this.serializedObject.FindProperty("subSectionsZ");
            _subSectionsCellOverlap = this.serializedObject.FindProperty("subSectionsCellOverlap");

            _generateHeightMap = this.serializedObject.FindProperty("generateHeightmap");
            _heightLookupType = this.serializedObject.FindProperty("heightLookupType");
            _heightLookupMaxDepth = this.serializedObject.FindProperty("heightLookupMaxDepth");
            _lowerBoundary = this.serializedObject.FindProperty("lowerBoundary");
            _upperBoundary = this.serializedObject.FindProperty("upperBoundary");
            _obstacleAndGroundDetection = this.serializedObject.FindProperty("obstacleAndGroundDetection");

            _storeBakedDataAsAsset = this.serializedObject.FindProperty("_storeBakedDataAsAsset");
            _automaticInitialization = this.serializedObject.FindProperty("_automaticInitialization");
            _automaticConnections = this.serializedObject.FindProperty("_automaticConnections");
            _connectorPortalWidth = this.serializedObject.FindProperty("_connectorPortalWidth");

            _heightStrategyMissing = (GameServices.heightStrategy == null);
        }
    }
}
