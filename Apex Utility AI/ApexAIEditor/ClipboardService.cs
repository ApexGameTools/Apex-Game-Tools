/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor
{
    using UnityEditor;
    using UnityEngine;

    internal static class ClipboardService
    {
        internal static void CopyToClipboard(AIUI sourceUI)
        {
            if (sourceUI.currentAction != null)
            {
                EditorGUIUtility.systemCopyBuffer = GuiSerializer.Serialize(sourceUI.currentAction);
            }
            else if (sourceUI.currentQualifier != null)
            {
                EditorGUIUtility.systemCopyBuffer = GuiSerializer.Serialize(sourceUI.currentQualifier);
            }
            else if (sourceUI.selectedViews.Count > 0)
            {
                EditorGUIUtility.systemCopyBuffer = GuiSerializer.Serialize(sourceUI.selectedViews);
            }
        }

        internal static void CutToClipboard(AIUI sourceUI)
        {
            if (sourceUI.currentAction != null)
            {
                EditorGUIUtility.systemCopyBuffer = GuiSerializer.Serialize(sourceUI.currentAction);
                sourceUI.RemoveAction(sourceUI.currentAction);
            }
            else if (sourceUI.currentQualifier != null)
            {
                if (sourceUI.currentQualifier.isDefault)
                {
                    EditorUtility.DisplayDialog("Invalid Action", "The default qualifier cannot be cut. Use copy instead.", "Ok");
                    return;
                }

                EditorGUIUtility.systemCopyBuffer = GuiSerializer.Serialize(sourceUI.currentQualifier);
                sourceUI.RemoveQualifier(sourceUI.currentQualifier);
            }
            else if (sourceUI.selectedViews.Count > 0)
            {
                var selectedViews = sourceUI.selectedViews;
                var cutCount = selectedViews.Count;
                for (int i = 0; i < cutCount; i++)
                {
                    var sv = selectedViews[i] as SelectorView;
                    if (sv != null && object.ReferenceEquals(sourceUI.rootSelector, sv.selector))
                    {
                        EditorUtility.DisplayDialog("Invalid Action", "The root selector can not be part of a cut operation.", "Ok");
                        return;
                    }
                }

                EditorGUIUtility.systemCopyBuffer = GuiSerializer.Serialize(sourceUI.selectedViews);

                using (sourceUI.undoRedo.bulkOperation)
                {
                    for (int i = cutCount - 1; i >= 0; i--)
                    {
                        sourceUI.RemoveView(selectedViews[i]);
                    }
                }
            }
        }

        internal static void Duplicate(AIUI sourceUI, Vector2 mousePos)
        {
            CopyToClipboard(sourceUI);
            PasteFromClipboard(sourceUI, mousePos);
        }

        internal static void PasteFromClipboard(AIUI targetUi, Vector2 mousePos)
        {
            if (string.IsNullOrEmpty(EditorGUIUtility.systemCopyBuffer))
            {
                // nothing in the clipboard
                Debug.LogWarning("Could not paste from clipboard; clipboard is empty.");
                return;
            }

            try
            {
                GuiSerializer.DeserializeSnippet(EditorGUIUtility.systemCopyBuffer, targetUi, mousePos, true);
                targetUi.isDirty = true;
            }
            catch
            {
                Debug.LogWarning("Could not paste from clipboard; invalid data detected.");
            }
        }
    }
}