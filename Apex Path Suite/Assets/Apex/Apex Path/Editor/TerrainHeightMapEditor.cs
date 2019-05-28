namespace Apex.Editor
{
    using Apex.WorldGeometry;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(TerrainHeightMap), false)]
    public class TerrainHeightMapEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            GUI.enabled = !EditorApplication.isPlaying;

            DrawDefaultInspector();

            GUI.enabled = true;
        }
    }
}
