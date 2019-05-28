namespace Apex.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(ApexComponentMaster), false), CanEditMultipleObjects]
    public class ApexComponentMasterEditor : Editor
    {
        private static GUIStyle _disabledLabel;

        private static GUIStyle disabledLabel
        {
            get
            {
                if (_disabledLabel == null)
                {
                    _disabledLabel = new GUIStyle(EditorStyles.label);
                    _disabledLabel.normal.textColor = Color.gray;
                }

                return _disabledLabel;
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField(new GUIContent(" Apex Components", UIResources.ApexLogo.texture));

            EditorGUI.indentLevel++;

            var master = this.target as ApexComponentMaster;
            foreach (var category in master.componentCategories)
            {
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.GetControlRect(true, 16f, EditorStyles.foldout);
                Rect foldRect = GUILayoutUtility.GetLastRect();

                category.isOpen = EditorGUI.Foldout(foldRect, category.isOpen, category.name, true);
                if (!category.isOpen)
                {
                    EditorGUILayout.EndVertical();
                    continue;
                }

                bool requiresCleanup = false;
                foreach (ApexComponentMaster.ComponentInfo c in category)
                {
                    if (c.component == null || c.component.Equals(null))
                    {
                        requiresCleanup = true;
                        continue;
                    }

                    var lblStyle = c.component.enabled ? EditorStyles.label : disabledLabel;

                    var visible = EditorGUILayout.ToggleLeft(c.name, c.isVisible, lblStyle);
                    if (visible != c.isVisible)
                    {
                        if (this.targets.Length > 1)
                        {
                            foreach (var t in this.targets)
                            {
                                ((ApexComponentMaster)t).Toggle(c.name, visible);
                            }
                        }
                        else
                        {
                            master.Toggle(c);
                        }

                        EditorUtility.SetDirty(master);
                    }
                }

                EditorGUILayout.EndVertical();

                if (requiresCleanup)
                {
                    foreach (var t in this.targets)
                    {
                        ((ApexComponentMaster)t).Cleanup();
                    }
                }
            }

            EditorGUI.indentLevel--;
            if (GUILayout.Button("Toggle All"))
            {
                foreach (var t in this.targets)
                {
                    var tmp = t as ApexComponentMaster;
                    tmp.ToggleAll();
                    EditorUtility.SetDirty(tmp);
                }
            }
        }

        private void OnEnable()
        {
            foreach (var t in this.targets)
            {
                var master = t as ApexComponentMaster;

                if (master.Init(GetCandidates(master)))
                {
                    EditorUtility.SetDirty(master);
                }
            }
        }

        private IEnumerable<ApexComponentMaster.ComponentCandidate> GetCandidates(ApexComponentMaster m)
        {
            var all = from c in m.GetComponents<MonoBehaviour>()
                      where c != null && !c.Equals(null)
                      select c;

            return from c in all
                   let t = c.GetType()
                   let aca = t.GetAttribute<ApexComponentAttribute>(true)
                   where aca != null
                   select new ApexComponentMaster.ComponentCandidate
                   {
                       component = c,
                       categoryName = aca.category
                   };
        }

        private void OnDestroy()
        {
            if (this.target == null)
            {
                foreach (var go in Selection.gameObjects)
                {
                    var allApex = from c in go.GetComponents<MonoBehaviour>()
                                  where Attribute.IsDefined(c.GetType(), typeof(ApexComponentAttribute))
                                  select c;

                    foreach (var c in allApex)
                    {
                        c.hideFlags &= ~HideFlags.HideInInspector;
                    }
                }
            }
        }
    }
}
