/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Apex.Editor;
    using Serialization;
    using UnityEditor;
    using UnityEngine;
    using Visualization;
    [InitializeOnLoad]
    public static class UnityEventListener
    {
        private static Action _pendingAction;

        static UnityEventListener()
        {
            EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemOnGUI;
#if UNITY_5 || UNITY_2017
            //This is only for Unity 5.2+
            Selection.selectionChanged += OnSelectionChanged;
#endif
        }

        private static IEnumerable<AIStorage> selectedAIs
        {
            get
            {
                return from o in Selection.objects
                       let ai = o as AIStorage
                       where ai != null
                       select ai;
            }
        }

        private static void OnSelectionChanged()
        {
            if (VisualizationManager.isVisualizing)
            {
                VisualizationManager.UpdateSelectedGameObjects(Selection.gameObjects);
            }
        }

        private static void OnProjectWindowItemOnGUI(string guid, Rect selectionRect)
        {
            if (_pendingAction != null)
            {
                return;
            }

            var evt = Event.current;
            if (evt == null)
            {
                return;
            }

            switch (evt.type)
            {
                case EventType.MouseDown:
                {
                    if (evt.clickCount == 2)
                    {
                        if (ExecuteCommand("Open", 100))
                        {
                            evt.Use();
                        }
                    }

                    return;
                }

                case EventType.KeyDown:
                {
                    //By default enter makes a multi selection become a single selection for some reason.
                    //So if at least one of the selected items is an AI we override that behavior by simply eating the event.
                    if ((evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return) && selectedAIs.Any())
                    {
                        evt.Use();
                    }

                    return;
                }

                case EventType.KeyUp:
                {
                    if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return)
                    {
                        if (ExecuteCommand("Open", 100))
                        {
                            evt.Use();
                        }
                    }

                    return;
                }

                case EventType.ExecuteCommand:
                {
                    if (ExecuteCommand(evt.commandName, 500))
                    {
                        evt.Use();
                    }

                    return;
                }
            }
        }

        private static bool ExecuteCommand(string cmd, int delay)
        {
            var ais = selectedAIs.ToArray();

            if (ais.Length == 0)
            {
                return false;
            }

            bool consumeEvent = false;

            switch (cmd)
            {
                case "Open":
                {
                    _pendingAction = () =>
                    {
                        for (int i = 0; i < ais.Length; i++)
                        {
                            AIEditorWindow.Open(ais[i].aiId);
                        }
                    };

                    consumeEvent = true;
                    break;
                }

                case "Delete":
                case "SoftDelete":
                {
                    //reload all windows with deleted ais
                    var aisBefore = StoredAIs.AIs.ToArray();
                    _pendingAction = () =>
                    {
                        StoredAIs.Refresh();
                        var aisAfter = StoredAIs.AIs.ToArray();
                        var deletedIds = aisBefore.Except(aisAfter).Select(ai => ai.aiId).ToArray();

                        AIEditorWindow.Unload(deletedIds);
                    };

                    break;
                }

                case "Duplicate":
                {
                    //identify all additions and re-ID them.
                    var aisBefore = StoredAIs.AIs.ToArray();
                    _pendingAction = () =>
                    {
                        StoredAIs.Refresh();
                        var aisAfter = StoredAIs.AIs.ToArray();
                        var addedAis = aisAfter.Except(aisBefore);
                        foreach (var copiedAI in addedAis)
                        {
                            var ui = AIUI.Load(copiedAI, false);
                            if (ui != null)
                            {
                                ui.ai.RegenerateIds();
                                copiedAI.aiId = ui.ai.id.ToString();
                                ui.Save(null);
                            }
                            else
                            {
                                var path = AssetDatabase.GetAssetPath(copiedAI);
                                AssetDatabase.DeleteAsset(path);
                            }
                        }
                    };

                    break;
                }
            }

            EditorAsync.ExecuteDelayed(
                () =>
                {
                    _pendingAction();
                    _pendingAction = null;
                },
                delay);

            return consumeEvent;
        }
    }
}
