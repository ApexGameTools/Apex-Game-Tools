/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor.Reflection
{
    using System.Collections.Generic;
    using UnityEditor;

    internal sealed class EditorFieldCategory
    {
        internal string name;
        internal IEditorField[] fields;

        private List<IEditorField> _shownFields;

        internal void Render(AIInspectorState state, DependencyChecker dependencyChecker)
        {
            EditorGUILayout.Separator();

            bool hasCategory = !string.IsNullOrEmpty(this.name);
            if (hasCategory)
            {
                if (_shownFields == null)
                {
                    _shownFields = new List<IEditorField>(fields.Length);
                }
                else
                {
                    _shownFields.Clear();
                }

                for (int i = 0; i < fields.Length; i++)
                {
                    var f = fields[i];
                    if (dependencyChecker.AreDependenciesSatisfied(f.memberName))
                    {
                        _shownFields.Add(f);
                    }
                }

                if (_shownFields.Count == 0)
                {
                    return;
                }

                EditorGUILayout.LabelField(this.name, EditorStyles.label);
                EditorGUI.indentLevel += 1;

                foreach (var f in _shownFields)
                {
                    f.RenderField(state);
                }

                EditorGUI.indentLevel -= 1;
            }
            else
            {
                for (int i = 0; i < fields.Length; i++)
                {
                    var f = fields[i];
                    if (dependencyChecker.AreDependenciesSatisfied(f.memberName))
                    {
                        fields[i].RenderField(state);
                    }
                }
            }
        }
    }
}