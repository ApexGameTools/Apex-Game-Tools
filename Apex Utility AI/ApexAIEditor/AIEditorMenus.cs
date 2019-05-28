namespace Apex.AI.Editor
{
    using System;
    using Apex.AI.Serialization;
    using Apex.AI.Visualization;
    using Components;
    using UnityEditor;
    using UnityEngine;

    internal static class AIEditorMenus
    {
        private static readonly string ctrlOrCmd = Application.platform == RuntimePlatform.OSXEditor ? "Cmd" : "Ctrl";

        [MenuItem("Tools/Apex/AI Editor", false, 150)]
        public static void OpenEditor()
        {
            AIEditorWindow.Open();
        }

        [MenuItem("Tools/Apex/AI Repair", false, 150)]
        public static void RepairAis()
        {
            RepairWindow.ShowWindow();
        }

        [MenuItem("Tools/Apex/AI Investigator", false, 150)]
        public static void InvestigateAis()
        {
            AIInvestigatorWindow.ShowWindow();
        }

        [MenuItem("Tools/Apex/AI Settings", false, 150)]
        public static void OpenSettings()
        {
            SettingsWindow.ShowWindow();
        }

        internal static void ShowCanvasMenu(AIUI ui, Vector2 mousePos)
        {
            var menu = new GenericMenu();
            var screenPos = EditorGUIUtility.GUIToScreenPoint(mousePos);

            menu.AddItem(new GUIContent("Add Selector"), false, () => AIEntitySelectorWindow.Get<Selector>(screenPos, (selectorType) => { ui.AddSelector(mousePos, selectorType); }));
            menu.AddItem(new GUIContent("Add AI Link"), false, () => AISelectorWindow.Get(screenPos, (storedAI) => { ui.AddAILink(mousePos, storedAI.aiId); }));

            AddSharedItems(menu, ui, true, mousePos);
        }

        internal static void ShowBlankCanvasMenu(Vector2 mousePos)
        {
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("New AI"), false, () => AIEditorWindow.NewInActive());
            menu.AddItem(new GUIContent("Load AI"), false, () => AIEditorWindow.LoadInActive(mousePos));

            menu.ShowAsContext();
        }

        internal static void ShowViewMenu(AIUI ui, Vector2 mousePos)
        {
            var menu = new GenericMenu();
            var screenPos = EditorGUIUtility.GUIToScreenPoint(mousePos);
            bool allowDelete = true;

            if (ui.currentAction != null)
            {
                var qv = ui.currentQualifier;
                Action<Type> cb = (t) =>
                {
                    var newInstance = (IAction)Activator.CreateInstance(t);
                    ui.ReplaceAction(qv, newInstance);
                };

                menu.AddItem(new GUIContent("Change Type"), false, () => AIEntitySelectorWindow.Get<IAction>(screenPos, cb));
            }
            else if (ui.currentQualifier != null)
            {
                string label = (ui.currentAction == null && ui.currentQualifier.actionView == null) ? "Add Action" : "Replace Action";
                menu.AddItem(new GUIContent(label), false, () => AIEntitySelectorWindow.Get<IAction>(screenPos, (actionType) => { ui.SetAction(actionType); }));
                menu.AddItem(new GUIContent("Add Qualifier (sibling)"), false, () => AIEntitySelectorWindow.Get<IQualifier>(screenPos, (qualifierType) => { ui.AddQualifier(qualifierType); }));

                var qv = ui.currentQualifier;
                if (qv.isDefault)
                {
                    allowDelete = false;
                }
                else
                {
                    Action<Type> cb = (t) =>
                    {
                        var newInstance = (IQualifier)Activator.CreateInstance(t);
                        ui.ReplaceQualifier(qv, newInstance);
                    };

                    menu.AddSeparator(string.Empty);
                    menu.AddItem(new GUIContent("Change Type"), false, () => AIEntitySelectorWindow.Get<IQualifier>(screenPos, cb));
                }
            }
            else if (ui.currentSelector != null)
            {
                var sv = ui.currentSelector;
                menu.AddItem(new GUIContent("Add Qualifier"), false, () => AIEntitySelectorWindow.Get<IQualifier>(screenPos, (qualifierType) => { ui.AddQualifier(qualifierType); }));
                menu.AddSeparator(string.Empty);

                if (sv.isRoot)
                {
                    allowDelete = false;
                }
                else
                {
                    menu.AddItem(new GUIContent("Set as Root"), false, () => ui.SetRoot(sv.selector));
                }

                Action<Type> cb = (t) =>
                {
                    var newInstance = (Selector)Activator.CreateInstance(t);
                    ui.ReplaceSelector(sv, newInstance);
                };

                menu.AddItem(new GUIContent("Change Type"), false, () => AIEntitySelectorWindow.Get<Selector>(screenPos, cb));
            }
            else if (ui.currentAILink != null)
            {
                var alv = ui.currentAILink;
                Action<AIStorage> cb = (ai) =>
                {
                    ui.ChangeAILink(alv, new Guid(ai.aiId));
                };

                menu.AddItem(new GUIContent("Change AI"), false, () => AISelectorWindow.Get(screenPos, cb));
            }

            AddSharedItems(menu, ui, allowDelete, mousePos);
        }

        internal static void ShowRuntimeViewMenu(AIUI ui, Vector2 mousePos)
        {
            if (!ui.isVisualizing)
            {
                return;
            }

            var menu = new GenericMenu();
            var screenPos = EditorGUIUtility.GUIToScreenPoint(mousePos);

            if (ui.currentQualifier != null)
            {
                var visualizedQualifier = (IQualifierVisualizer)ui.currentQualifier.qualifier;
                if (visualizedQualifier.isBreakPoint)
                {
                    menu.AddItem(new GUIContent("Remove Breakpoint"), false, () => visualizedQualifier.isBreakPoint = false);
                }
                else
                {
                    menu.AddItem(new GUIContent("Set Breakpoint"), false, () => visualizedQualifier.isBreakPoint = true);
                }

                menu.AddItem(new GUIContent("Set Conditional Breakpoint"), false, () => BreakpointConditionWindow.Open(screenPos, visualizedQualifier));
            }
            else if (ui.currentSelector != null)
            {
                var visualizedSelector = (SelectorVisualizer)ui.currentSelector.selector;
                menu.AddItem(new GUIContent("Clear Breakpoints"), false, () => visualizedSelector.ClearBreakpoints());
            }
            else if (ui.currentAILink == null)
            {
                var visualizedAI = (UtilityAIVisualizer)ui.visualizedAI;
                menu.AddItem(new GUIContent("Clear All Breakpoints"), false, () => visualizedAI.ClearBreakpoints());
            }

            menu.ShowAsContext();
        }

        private static void AddSharedItems(GenericMenu menu, AIUI ui, bool allowDelete, Vector2 mousePos)
        {
            if (menu.GetItemCount() > 0)
            {
                menu.AddSeparator(string.Empty);
            }

            if (ui.undoRedo.canUndo)
            {
                menu.AddItem(new GUIContent(string.Concat("Undo (", ctrlOrCmd, " + Shift + Z)")), false, () => ui.undoRedo.Undo());
            }
            else
            {
                menu.AddDisabledItem(new GUIContent(string.Concat("Undo (", ctrlOrCmd, " + Shift + Z)")));
            }

            if (ui.undoRedo.canRedo)
            {
                menu.AddItem(new GUIContent(string.Concat("Redo (", ctrlOrCmd, " + Shift + Y)")), false, () => ui.undoRedo.Redo());
            }
            else
            {
                menu.AddDisabledItem(new GUIContent(string.Concat("Redo (", ctrlOrCmd, " + Shift + Y)")));
            }

            menu.AddSeparator(string.Empty);
            var hasSelection = ui.selectedViews.Count > 0 || ui.currentAction != null || ui.currentQualifier != null || ui.currentSelector != null;
            if (hasSelection)
            {
                menu.AddItem(new GUIContent(string.Concat("Cut (", ctrlOrCmd, " + X)")), false, () => ClipboardService.CutToClipboard(ui));
                menu.AddItem(new GUIContent(string.Concat("Copy (", ctrlOrCmd, " + C)")), false, () => ClipboardService.CopyToClipboard(ui));
                menu.AddItem(new GUIContent(string.Concat("Duplicate (", ctrlOrCmd, " + D)")), false, () => ClipboardService.Duplicate(ui, mousePos));
            }
            else
            {
                menu.AddDisabledItem(new GUIContent(string.Concat("Cut (", ctrlOrCmd, " + X)")));
                menu.AddDisabledItem(new GUIContent(string.Concat("Copy (", ctrlOrCmd, " + C)")));
                menu.AddDisabledItem(new GUIContent(string.Concat("Duplicate (", ctrlOrCmd, " + D)")));
            }

            if (!string.IsNullOrEmpty(EditorGUIUtility.systemCopyBuffer))
            {
                menu.AddItem(new GUIContent(string.Concat("Paste (", ctrlOrCmd, " + V)")), false, () => ClipboardService.PasteFromClipboard(ui, mousePos));
            }
            else
            {
                menu.AddDisabledItem(new GUIContent(string.Concat("Paste (", ctrlOrCmd, " + V)")));
            }

            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent(string.Concat("Select All (", ctrlOrCmd, " + A)")), false, () => ui.MultiSelectViews(ui.canvas.views));

            if (allowDelete)
            {
                menu.AddSeparator(string.Empty);
                if (hasSelection)
                {
                    menu.AddItem(new GUIContent("Delete (Del)"), false, () => ui.RemoveSelected());
                }
                else
                {
                    menu.AddDisabledItem(new GUIContent("Delete (Del)"));
                }
            }

            menu.ShowAsContext();
        }
    }
}