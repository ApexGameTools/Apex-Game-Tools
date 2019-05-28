/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Examples.Editor
{
    using Apex.Editor;
    using Apex.Examples.CostStrategy.FixedCosts;
    using UnityEditor;
    using UnityEngine;

    [CustomPropertyDrawer(typeof(CellCostType))]
    public class CellCostTypePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorUtilitiesInternal.EnumToIntField(position, property, typeof(CellCostType), label);
        }
    }
}
