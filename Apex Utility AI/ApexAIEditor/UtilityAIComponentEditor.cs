/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Apex.AI.Components;
    using Apex.Editor;
    using Serialization;
    using UnityEditor;
    using UnityEngine;
    using Utilities;

    [CustomEditor(typeof(UtilityAIComponent), false)]
    public class UtilityAIComponentEditor : Editor
    {
        private static readonly GUIContent _intervalLabel = new GUIContent("Interval", "The interval between each run of the AI. If the two values differ, the actual interval will be a random value between the two which is reevaluated for each run.");
        private static readonly GUIContent _delayLabel = new GUIContent("Start Delay", "The delay before the initial run of the AI. If the two values differ, the actual delay will be a random value between the two.");
        private static readonly GUIContent _openAITooltip = new GUIContent(string.Empty, "Open in Editor");

        private List<KeyValuePair<string, Type>> _contextProviderList;
        private string[] _contextProviderNames;
        private UtilityAIComponent _target;
        private string[] _aiNames;
        private string _nameSpace;

        public override void OnInspectorGUI()
        {
            EditorStyling.InitScaleAgnosticStyles();

            var isPlaying = Application.isPlaying;

            EditorGUILayout.Separator();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("AIs");
            EditorGUILayout.Separator();
            if (!isPlaying && GUILayout.Button(SharedStyles.addTooltip, SharedStyles.BuiltIn.addButtonSmall))
            {
                AddNew();
            }

            EditorGUILayout.EndHorizontal();

            int removeIdx = -1;
            if (_target.aiConfigs != null && _target.aiConfigs.Length > 0)
            {
                EditorGUILayout.BeginVertical("Box");
                EditorGUIUtility.labelWidth = 80f;

                int aiCount = _target.aiConfigs.Length;
                for (int i = 0; i < aiCount; i++)
                {
                    if (DrawAIItem(i))
                    {
                        removeIdx = i;
                    }
                }

                EditorGUIUtility.labelWidth = 0f;
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Separator();

            if (!isPlaying)
            {
                ShowContextSelector();
                EditorGUILayout.Separator();
            }

            if (removeIdx >= 0)
            {
                Delete(removeIdx);
            }
        }

        private bool DrawAIItem(int idx)
        {
            var result = false;
            var isPlaying = Application.isPlaying;
            var aiCfg = _target.aiConfigs[idx];

            // Item Header
            EditorGUILayout.BeginHorizontal(SharedStyles.BuiltIn.listItemHeader);

            var isActive = EditorGUILayout.ToggleLeft(string.Empty, aiCfg.isActive);
            if (isActive != aiCfg.isActive)
            {
                _target.ToggleActive(idx, isActive);
                EditorUtility.SetDirty(_target);

                //We want the visualizer to update
                AIEditorWindow.UpdateVisualizedEntities();
            }

            if (GUILayout.Button(_openAITooltip, EditorStyling.Skinned.viewButtonSmall))
            {
                GUI.changed = false;

                AIEditorWindow.Open(aiCfg.aiId);
                Selection.activeGameObject = _target.gameObject;
            }

            GUILayout.Space(4f);
            if (!isPlaying && GUILayout.Button(SharedStyles.deleteTooltip, SharedStyles.BuiltIn.deleteButtonSmall))
            {
                result = true;

                // We do not want the button click itself to count as a change. Since no other changes are expected when a button click is encountered, we can just set it to false.
                GUI.changed = false;
            }

            EditorGUILayout.EndHorizontal();

            // Item fields
            GUI.enabled = !isPlaying;
            DrawAISelector(idx);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(_intervalLabel, GUILayout.Width(80f));
            aiCfg.intervalMin = _target.FloatField(aiCfg.intervalMin, null, GUILayout.Width(40f));
            EditorGUILayout.LabelField("to", GUILayout.Width(20f));
            aiCfg.intervalMax = _target.FloatField(aiCfg.intervalMax, null, GUILayout.Width(40f));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(_delayLabel, GUILayout.Width(80f));
            aiCfg.startDelayMin = _target.FloatField(aiCfg.startDelayMin, null, GUILayout.Width(40f));
            EditorGUILayout.LabelField("to", GUILayout.Width(20f));
            aiCfg.startDelayMax = _target.FloatField(aiCfg.startDelayMax, null, GUILayout.Width(40f));
            EditorGUILayout.EndHorizontal();

            GUI.enabled = true;

            EditorGUILayout.Separator();

            return result;
        }

        private void DrawAISelector(int idx)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("AI", _aiNames[idx]);

            if (GUILayout.Button(SharedStyles.changeSelectionTooltip, SharedStyles.BuiltIn.changeButtonSmall))
            {
                GUI.changed = false;

                var aiCfg = _target.aiConfigs[idx];
                var screenPos = EditorGUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                var win = AISelectorWindow.Get(
                            screenPos,
                            (ai) =>
                            {
                                if (aiCfg.aiId != ai.aiId)
                                {
                                    if (_target.aiConfigs.Any(cfg => cfg.aiId == ai.aiId))
                                    {
                                        EditorUtility.DisplayDialog("Duplicate AI", "The selected AI is already part of this client.", "OK");
                                        return;
                                    }

                                    _aiNames[idx] = ai.name;
                                    aiCfg.aiId = ai.aiId;
                                    EditorUtility.SetDirty(_target);
                                }
                            });

                var curSelectedId = aiCfg.aiId;
                win.Preselect((ai) => ai.aiId == curSelectedId);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void ShowContextSelector()
        {
            object cp = _target.As<IContextProvider>();
            if (cp == null)
            {
                if (_contextProviderList == null)
                {
                    _contextProviderList = new List<KeyValuePair<string, Type>>();

                    _contextProviderList = (from t in ApexReflection.GetRelevantTypes()
                                            where typeof(IContextProvider).IsAssignableFrom(t) && !t.IsAbstract
                                            orderby t.Name
                                            select new KeyValuePair<string, Type>(t.Name, t)).ToList();

                    _contextProviderNames = _contextProviderList.Select(p => p.Key).ToArray();
                }

                EditorGUILayout.Separator();
                var style = new GUIStyle(GUI.skin.label);
                style.normal.textColor = Color.yellow;

                if (_contextProviderList.Count == 0)
                {
                    EditorGUILayout.LabelField("A Context Provider is required!", style);
                    EditorGUILayout.BeginHorizontal();
                    _nameSpace = EditorGUILayout.TextField("Namespace", _nameSpace);
                    if (GUILayout.Button("Create a Context Provider"))
                    {
                        ContextGenerator.WriteContextFiles(_nameSpace);
                        EditorUtility.DisplayDialog("Context Created", "A basic Context and Context provider has been created in the root of the Assets folder.\n\nPlease review the documentation for additional details on Contexts.", "Ok");
                    }

                    EditorGUILayout.EndHorizontal();
                }
                else if (_contextProviderList.Count == 1)
                {
                    _target.gameObject.AddComponent(_contextProviderList[0].Value);
                }
                else
                {
                    EditorGUILayout.LabelField("Select a Context Provider", style);
                    var selectedIdx = EditorGUILayout.Popup(-1, _contextProviderNames);
                    if (selectedIdx >= 0)
                    {
                        _target.gameObject.AddComponent(_contextProviderList[selectedIdx].Value);
                    }
                }
            }
        }

        private void AddNew()
        {
            var screenPos = EditorGUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            AISelectorWindow.Get(screenPos, DoAddNew);
        }

        private void DoAddNew(AIStorage[] ais)
        {
            var currentAIs = _target.aiConfigs ?? Empty<UtilityAIConfig>.array;
            var newAis = (from ai in ais
                          where !currentAIs.Any(cfg => cfg.aiId == ai.aiId)
                          select ai).ToArray();

            var itemCount = newAis.Length;
            if (itemCount == 0)
            {
                return;
            }

            _target.aiConfigs = GrowArray(_target.aiConfigs, itemCount);
            _aiNames = GrowArray(_aiNames, itemCount);

            int targetIdx = currentAIs.Length;
            for (int i = 0; i < itemCount; i++)
            {
                var addedAI = newAis[i];
                _target.aiConfigs[targetIdx] = new UtilityAIConfig
                {
                    aiId = addedAI.aiId,
                    intervalMin = 1f,
                    intervalMax = 1f,
                    startDelayMin = 0f,
                    startDelayMax = 0f,
                    isActive = true
                };

                _aiNames[targetIdx] = addedAI.name;

                targetIdx++;
            }

            EditorUtility.SetDirty(_target);
        }

        private void Delete(int idx)
        {
            _target.aiConfigs = ShrinkArray(_target.aiConfigs, idx);
            _aiNames = ShrinkArray(_aiNames, idx);

            EditorUtility.SetDirty(_target);
        }

        private T[] GrowArray<T>(T[] array, int increase)
        {
            if (array == null || array.Length == 0)
            {
                return new T[increase];
            }

            var newArr = new T[array.Length + increase];
            Array.Copy(array, newArr, array.Length);

            return newArr;
        }

        private T[] ShrinkArray<T>(T[] array, int idx)
        {
            if (array == null || array.Length == 1)
            {
                return null;
            }

            var newArr = new T[array.Length - 1];
            Array.Copy(array, 0, newArr, 0, idx);
            Array.Copy(array, idx + 1, newArr, idx, array.Length - idx - 1);

            return newArr;
        }

        private void Init()
        {
            if (_target == null)
            {
                _target = (UtilityAIComponent)this.target;
            }

            var currentSelectedAIs = _target.aiConfigs;
            if (currentSelectedAIs == null)
            {
                return;
            }

            _aiNames = new string[currentSelectedAIs.Length];

            for (int i = 0; i < currentSelectedAIs.Length; i++)
            {
                var ai = currentSelectedAIs[i] != null ? StoredAIs.GetById(currentSelectedAIs[i].aiId) : null;
                if (ai != null)
                {
                    _aiNames[i] = ai.name;
                }
                else
                {
                    _aiNames[i] = "?";
                }
            }
        }

        private void OnEnable()
        {
            Init();

            _target.OnNewAI += UtilityAIComponent_OnNewAI;
        }

        private void OnDisable()
        {
            _target.OnNewAI -= UtilityAIComponent_OnNewAI;
        }

        private void UtilityAIComponent_OnNewAI(IUtilityAIClient client)
        {
            Init();
        }
    }
}