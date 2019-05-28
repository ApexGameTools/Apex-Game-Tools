/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Editor
{
    using UnityEditor;

    [CustomEditor(typeof(ApexQuickStartComponent), true)]
    public class ApexQuickStartEditor : Editor
    {
        private void OnEnable()
        {
            var qs = this.target as ApexQuickStartComponent;
            var prefab = PrefabUtility.GetPrefabType(qs.gameObject);
            var isPrefab = (prefab == PrefabType.Prefab || prefab == PrefabType.ModelPrefab);

            var go = qs.Apply(isPrefab);

            if (go != null)
            {
                Selection.activeGameObject = go;
            }

            DestroyImmediate(qs, true);
        }
    }
}
