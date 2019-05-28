namespace Apex.Editor
{
    using Apex.DataStructures;
using Apex.WorldGeometry;
using UnityEditor;
using UnityEngine;

    public class GridSticherUtilityWindow : EditorWindow
    {
        private int _fieldSizeX = 4;
        private int _fieldSizeZ = 4;
        private bool _disableAutoInit = true;

        private string _friendlyName = "Grid";
        private int _sizeX = 10;
        private int _sizeZ = 10;
        private float _cellSize = 2f;
        private float _connectorPortalWidth = 0f;
        private float _obstacleSensitivityRange = 0.5f;
        private int _subSectionsX = 2;
        private int _subSectionsZ = 2;
        private int _subSectionsCellOverlap = 2;
        private bool _generateHeightMap = true;
        private HeightLookupType _heightLookupType = HeightLookupType.QuadTree;
        private int _heightLookupMaxDepth = 5;
        private float _lowerBoundary = 1f;
        private float _upperBoundary = 10f;

        private void OnGUI()
        {
            var rect = EditorGUILayout.BeginVertical(GUILayout.Width(400), GUILayout.Height(400));
            EditorGUIUtility.labelWidth = 200f;

            EditorGUILayout.HelpBox("This tool allows you to easily generate a grid field, that is a number of grids stitched together in order to cover a large scene", MessageType.Info);
            EditorGUILayout.Separator();

            EditorUtilities.Section("Field");

            _fieldSizeX = EditorGUILayout.IntField(new GUIContent("Grids along x-axis", "Number of grids along the x-axis."), _fieldSizeX);
            _fieldSizeZ = EditorGUILayout.IntField(new GUIContent("Grids along z-axis", "Number of grids along the x-axis."), _fieldSizeZ);
            _disableAutoInit = EditorGUILayout.Toggle(new GUIContent("Disable Automatic Initialization", "Sets the grid's automatic initialization setting to false."), _disableAutoInit);

            EditorGUILayout.Separator();
            _friendlyName = EditorGUILayout.TextField(new GUIContent("Grid Base Name", "An optional friendly name for the grids, each grid will also get a number post fix."), _friendlyName);

            EditorUtilities.Section("Layout");

            _sizeX = EditorGUILayout.IntField(new GUIContent("Size X", "Number of cells along the x-axis."), _sizeX);
            _sizeZ = EditorGUILayout.IntField(new GUIContent("Size Z", "Number of cells along the z-axis."), _sizeZ);

            _cellSize = EditorGUILayout.FloatField(new GUIContent("Cell Size", "The size of each grid cell, expressed as the length of one side."), _cellSize);
            _connectorPortalWidth = EditorGUILayout.FloatField(new GUIContent("Connector Portal Width", "The width of the connector portals.\nIf left at 0 it will default to the cellSize, but if larger units need to be able to traverse a connector the width must be set accordingly."), _connectorPortalWidth);
            _lowerBoundary = EditorGUILayout.FloatField(new GUIContent("Lower Boundary", "How far below the grid's plane does the grid have influence."), _lowerBoundary);
            _upperBoundary = EditorGUILayout.FloatField(new GUIContent("Upper Boundary", "How far above the grid's plane does the grid have influence."), _upperBoundary);
            _obstacleSensitivityRange = EditorGUILayout.FloatField(new GUIContent("Obstacle Sensitivity Range", "How close to the center of a cell must an obstacle be to block the cell."), _obstacleSensitivityRange);

            if (_obstacleSensitivityRange > (_cellSize / 2.0f))
            {
                _obstacleSensitivityRange = _cellSize / 2.0f;
            }

            EditorUtilities.Section("Subsections");

            _subSectionsX = EditorGUILayout.IntField(new GUIContent("Subsections X", "Number of subsections along the x-axis."), _subSectionsX);
            _subSectionsZ = EditorGUILayout.IntField(new GUIContent("Subsections Z", "Number of subsections along the z-axis."), _subSectionsZ);
            _subSectionsCellOverlap = EditorGUILayout.IntField(new GUIContent("Subsections Cell Overlap", "The number of cells by which subsections overlap neighbouring subsections."), _subSectionsCellOverlap);

            EditorGUI.indentLevel -= 1;
            EditorGUILayout.Separator();
            _generateHeightMap = EditorGUILayout.Toggle(new GUIContent("Generate Height Map", "Controls whether the grid generates a height map to allow height sensitive navigation."), _generateHeightMap);
            EditorGUI.indentLevel += 1;

            if (_generateHeightMap)
            {
                _heightLookupType = (HeightLookupType)EditorGUILayout.EnumPopup(new GUIContent("Lookup Type", "Dictionaries are fast but dense. Quad Trees are sparse but slower and are very dependent on height distributions. Use Quad trees on maps with large same height areas."), _heightLookupType);
                if (_heightLookupType == HeightLookupType.QuadTree)
                {
                    _heightLookupMaxDepth = EditorGUILayout.IntField(new GUIContent("Tree Depth", "The higher the allowed depth, the more sparse the tree will be but it will also get slower."), _heightLookupMaxDepth);
                }
            }

            EditorGUILayout.Separator();
            if (GUILayout.Button("Create Grid Field"))
            {
                CreateGridField();

                this.Close();
            }

            EditorGUILayout.Separator();
            EditorGUIUtility.labelWidth = 0f;
            EditorGUILayout.EndVertical();
            this.minSize = new Vector2(rect.width, rect.height);
        }

        private void CreateGridField()
        {
            var root = new GameObject("Grids");

            float gridWidth = _sizeX * _cellSize;
            float gridDepth = _sizeZ * _cellSize;

            float startX = -((_fieldSizeX - 1) * 0.5f) * gridWidth;
            float startZ = -((_fieldSizeZ - 1) * 0.5f) * gridDepth;

            float portalWidth = _connectorPortalWidth > 0f ? _connectorPortalWidth : _cellSize;

            for (int x = 0; x < _fieldSizeX; x++)
            {
                for (int z = 0; z < _fieldSizeZ; z++)
                {
                    var name = string.Format("{0}_{1}_{2}", _friendlyName, x, z);

                    var goGrid = new GameObject(name);
                    goGrid.transform.parent = root.transform;
                    goGrid.transform.localPosition = new Vector3(startX + (x * gridWidth), 0f, startZ + (z * gridDepth));
                                        
                    //Init the grid
                    var g = goGrid.AddComponent<GridComponent>();
                    g.friendlyName = name;
                    g.sizeX = _sizeX;
                    g.sizeZ = _sizeZ;
                    g.cellSize = _cellSize;
                    g.connectorPortalWidth = _connectorPortalWidth;
                    g.obstacleSensitivityRange = _obstacleSensitivityRange;
                    g.subSectionsX = _subSectionsX;
                    g.subSectionsZ = _subSectionsZ;
                    g.subSectionsCellOverlap = _subSectionsCellOverlap;
                    g.generateHeightmap = _generateHeightMap;
                    g.heightLookupType = _heightLookupType;
                    g.heightLookupMaxDepth = _heightLookupMaxDepth;
                    g.lowerBoundary = _lowerBoundary;
                    g.upperBoundary = _upperBoundary;
                    g.automaticInitialization = !_disableAutoInit;

                    //Add portals
                    if (x < _fieldSizeX - 1 || z < _fieldSizeZ - 1)
                    {
                        var goPortals = new GameObject("Portals");
                        goPortals.transform.parent = goGrid.transform;
                        goPortals.transform.localPosition = Vector3.zero;

                        if (x < _fieldSizeX - 1)
                        {
                            var centerX = (gridWidth - portalWidth) * 0.5f;
                            var centerZ = 0f;

                            ConstructPortal(
                                goPortals,
                                new Bounds(new Vector3(centerX, 0f, centerZ), new Vector3(portalWidth - 0.05f, 0.1f, gridDepth)),
                                new Bounds(new Vector3(centerX + portalWidth, 0f, centerZ), new Vector3(portalWidth - 0.05f, 0.1f, gridDepth)),
                                "RightPortal");
                        }

                        if (z < _fieldSizeZ - 1)
                        {
                            var centerX = 0f;
                            var centerZ = (gridDepth - portalWidth) * 0.5f;

                            ConstructPortal(
                                goPortals,
                                new Bounds(new Vector3(centerX, 0f, centerZ), new Vector3(gridWidth, 0.1f, portalWidth - 0.05f)),
                                new Bounds(new Vector3(centerX, 0f, centerZ + portalWidth), new Vector3(gridWidth, 0.1f, portalWidth - 0.05f)),
                                "TopPortal");
                        }
                    }       
                }
            }
        }

        private void ConstructPortal(GameObject parent, Bounds one, Bounds two, string name)
        {
            var portal = parent.AddComponent<GridPortalComponent>();

            portal.relativeToTransform = true;
            portal.portalName = name;
            portal.type = PortalType.Connector;
            portal.portalOne = one;
            portal.portalTwo = two;
        }
    }
}
