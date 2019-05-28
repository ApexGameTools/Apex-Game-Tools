namespace Apex.Editor
{
    using Apex.Editor.Versioning;
    using UnityEditor;

    public static class SharedMenuExtentions
    {
        [MenuItem("Tools/Apex/Products", false, 200)]
        public static void ProductsWindow()
        {
            EditorWindow.GetWindow<ProductsWindow>(true, "Apex - Products");
        }

        [MenuItem("Tools/Apex/Upgrade", false, 300)]
        public static void CleanupMenu()
        {
            VersionUpgraderWindow.ShowWindow();
        }
    }
}
