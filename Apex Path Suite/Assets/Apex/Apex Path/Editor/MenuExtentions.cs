namespace Apex.Editor
{
    using UnityEditor;

    public static class MenuExtentions
    {
        [MenuItem("GameObject/Create Other/Apex/Game World")]
        public static void GameWorldMenu()
        {
            QuickStarts.GameWorld(null);
        }

        [MenuItem("Tools/Apex/Attributes Utility", false, 100)]
        public static void AttributesUtility()
        {
            EditorWindow.GetWindow<AttributesUtilityWindow>(true, "Apex Path - Attributes Utility");
        }

        [MenuItem("Tools/Apex/Grid Field Utility", false, 100)]
        public static void GridFieldUtility()
        {
            EditorWindow.GetWindow<GridSticherUtilityWindow>(true, "Apex Path - Grid Field Utility");
        }
    }
}
