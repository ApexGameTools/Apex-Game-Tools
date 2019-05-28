/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Editor.Reflection
{
    using System;

    internal sealed class EditorItem
    {
        internal object item;
        internal string name;
        internal EditorFieldCategory[] fieldCategories;
        internal DependencyChecker dependencyChecker;

        internal void Render(AIInspectorState state)
        {
            for (int i = 0; i < fieldCategories.Length; i++)
            {
                fieldCategories[i].Render(state, dependencyChecker);
            }
        }
    }
}
