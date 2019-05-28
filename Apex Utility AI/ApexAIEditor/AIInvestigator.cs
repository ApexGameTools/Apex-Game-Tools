/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Apex.Editor;
    using Components;
    using Serialization;
    using UnityEditor;
#if UNITY_2017
    using UnityEditor.SceneManagement;
#endif
    using UnityEngine;
    using Utilities;

    internal class AIInvestigator
    {
        private static readonly string[] _scenesTabTitles = new string[] { "Current Scene", "Selected Scenes", "All Scenes" };
        private static readonly string[] _aiGroupingTabTitles = new string[] { "Scene or Prefab", "AI" };

        private EditorWindow _owner;
        private string _progressMessage;
        private bool _cancelled;
        private GUIContent _listItemContent = new GUIContent();
        private GUIContent _tooltippedLabel = new GUIContent();

        private Vector2 _sceneScrollPos;
        private Vector2 _aiScrollPos;
        private ListView<string> _sceneSelectView;
        private IEnumerable<string> _sceneList;
        private bool _includePrefabs = true;
        private Step _aiStep;
        private AIGrouping _aiGrouping;
        private Scenes _scenes = Scenes.Selected;
        private Dictionary<string, AIReference> _aiReferences;
        private List<SceneOrPrefab> _referencers;
        private AIReference[] _aisList;

        internal AIInvestigator(EditorWindow owner)
        {
            _owner = owner;
        }

        private enum Step
        {
            Start,
            Processing,
            Result
        }

        private enum AIGrouping
        {
            ScenesAndPrefabs,
            AIs
        }

        private enum Scenes
        {
            Current,
            Selected,
            All
        }

        private ListView<string> sceneListView
        {
            get
            {
                if (_sceneSelectView == null)
                {
                    _sceneSelectView = new ListView<string>(_owner, RenderListItem, MatchItem, false, true, (ignored) => CreateOverview());
                    _sceneList = GetScenePaths(Scenes.All);
                }

                return _sceneSelectView;
            }
        }

        internal void Reset()
        {
            _scenes = Scenes.Selected;
            _aiScrollPos = _sceneScrollPos = Vector2.zero;
            _sceneSelectView = null;
            _aiStep = Step.Start;
        }

        internal void Render()
        {
            switch (_aiStep)
            {
                case Step.Start:
                {
                    DrawAIOverviewStart();
                    break;
                }

                case Step.Processing:
                {
                    DrawAIOverviewWorking();
                    break;
                }

                case Step.Result:
                {
                    DrawAIOverviewComplete();
                    break;
                }
            }
        }

        private static bool OpenScene(string scene)
        {
#if UNITY_2017
            var s = EditorSceneManager.OpenScene(scene);
            return s.IsValid();
#else
            return EditorApplication.OpenScene(scene);
#endif
        }

        private static string GetCurrentScene()
        {
#if UNITY_2017
            var scene = EditorSceneManager.GetActiveScene();
            return scene.path;
#else
            return EditorApplication.currentScene;
#endif
        }

        private GUIContent RenderListItem(string s)
        {
            _listItemContent.text = s;
            return _listItemContent;
        }

        private bool MatchItem(string s, string search)
        {
            search = search.Replace(" ", string.Empty);
            var name = s.Replace(" ", string.Empty);
            return name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void DrawAIOverviewStart()
        {
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("This tool allows you to get an overview of which AIs are in use across scenes and prefabs in the project.\n\nThe tool can identify AIs referenced by Utility AI Components or other AIs, but not references that exist only in code.", SharedStyles.BuiltIn.wrappedText);
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Select which scene(s) to investigate.", SharedStyles.BuiltIn.centeredWrappedText);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            _scenes = (Scenes)GUILayout.SelectionGrid((int)_scenes, _scenesTabTitles, 3, EditorStyles.toolbarButton);
            EditorGUILayout.Separator();
            GUILayout.FlexibleSpace();
            _tooltippedLabel.text = "Include prefabs";
            _tooltippedLabel.tooltip = "Will include all prefabs regardless of them being referenced by a scene or not.";
            _includePrefabs = GUILayout.Toggle(_includePrefabs, _tooltippedLabel, EditorStyles.toolbarButton);
            EditorGUILayout.EndHorizontal();

            if (_scenes == Scenes.Selected)
            {
                this.sceneListView.Render(_sceneList);
            }
            else
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("Fixed Selection.", SharedStyles.BuiltIn.centeredWrappedText);
                GUILayout.FlexibleSpace();
            }

            if (GUILayout.Button("Create Overview"))
            {
                CreateOverview();
            }
        }

        private void CreateOverview()
        {
#if UNITY_2017
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
#else
            EditorApplication.SaveCurrentSceneIfUserWantsTo();
#endif
            _aiStep = Step.Processing;
            EditorAsync.ExecuteOnMain(
                ResolveReferences(),
                10,
                () =>
                {
                    if (_cancelled)
                    {
                        _cancelled = false;
                        _aiStep = Step.Start;
                    }
                    else
                    {
                        _aiStep = Step.Result;
                    }

                    _owner.Repaint();
                });

            _owner.Repaint();
        }

        private void DrawAIOverviewWorking()
        {
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Working on it....", SharedStyles.BuiltIn.centeredWrappedText);
            EditorGUILayout.LabelField(_progressMessage, SharedStyles.BuiltIn.centeredWrappedText);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Cancel"))
            {
                _cancelled = true;
            }
        }

        private void DrawAIOverviewComplete()
        {
            if (_referencers == null)
            {
                _aiStep = Step.Start;
                return;
            }

            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Below you can see which AIs are in use in which scenes and prefabs.", SharedStyles.BuiltIn.wrappedText);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button("Expand All", EditorStyles.toolbarButton))
            {
                _referencers.Apply(s => s.indirectExpanded = s.directExpanded = true);
                _aisList.Apply(ai => ai.directReferencesExpanded = ai.indirectReferencesExpanded = ai.directAIsExpanded = ai.indirectAIsExpanded = ai.directScenesOrPrefabsExpanded = ai.indirectScenesOrPrefabsExpanded = true);
            }

            if (GUILayout.Button("Collapse All", EditorStyles.toolbarButton))
            {
                _referencers.Apply(s => s.indirectExpanded = s.directExpanded = false);
                _aisList.Apply(ai => ai.directReferencesExpanded = ai.indirectReferencesExpanded = ai.directAIsExpanded = ai.indirectAIsExpanded = ai.directScenesOrPrefabsExpanded = ai.indirectScenesOrPrefabsExpanded = false);
            }

            if (GUILayout.Button("Copy", EditorStyles.toolbarButton))
            {
                EditorGUIUtility.systemCopyBuffer = Export();
            }

            GUILayout.FlexibleSpace();

            EditorGUILayout.LabelField("Group by", GUILayout.Width(60f));
            _aiGrouping = (AIGrouping)GUILayout.SelectionGrid((int)_aiGrouping, _aiGroupingTabTitles, 2, EditorStyles.toolbarButton);
            EditorGUILayout.EndHorizontal();

            if (_aiGrouping == AIGrouping.ScenesAndPrefabs)
            {
                _sceneScrollPos = EditorGUILayout.BeginScrollView(_sceneScrollPos, "Box");
                EditorGUILayout.LabelField("Scenes", SharedStyles.BuiltIn.centeredWrappedText);
                foreach (var scene in _referencers.Where(item => !item.isPrefab))
                {
                    DrawSceneOrPrefabListItem(scene);
                }

                EditorGUILayout.Separator();
                EditorGUILayout.LabelField("Prefabs", SharedStyles.BuiltIn.centeredWrappedText);
                foreach (var prefab in _referencers.Where(item => item.isPrefab))
                {
                    DrawSceneOrPrefabListItem(prefab);
                }

                EditorGUILayout.EndScrollView();
            }
            else
            {
                _aiScrollPos = EditorGUILayout.BeginScrollView(_aiScrollPos, "Box");
                EditorGUILayout.LabelField("AIs", SharedStyles.BuiltIn.centeredWrappedText);
                foreach (var aiRef in _aisList)
                {
                    DrawAIListItem(aiRef);
                }

                EditorGUILayout.EndScrollView();
            }

            if (GUILayout.Button("Back"))
            {
                _aiScrollPos = _sceneScrollPos = Vector2.zero;
                _aiStep = Step.Start;
            }
        }

        private void DrawSceneOrPrefabListItem(SceneOrPrefab item)
        {
            var labelContent = new GUIContent();

            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.BeginHorizontal(SharedStyles.BuiltIn.listItemHeader);
            labelContent.text = item.name;
            EditorGUILayout.LabelField(labelContent);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical("Box");
            labelContent.text = "Direct References";
            EditorGUILayout.LabelField(labelContent, SharedStyles.BuiltIn.centeredWrappedText);
            item.directExpanded = EditorGUI.Foldout(GUILayoutUtility.GetLastRect(), item.directExpanded, GUIContent.none, true);
            if (item.directExpanded)
            {
                foreach (var aiRef in item.directReferences)
                {
                    labelContent.text = aiRef.name;
                    EditorGUILayout.LabelField(labelContent);
                }
            }

            EditorGUILayout.EndVertical();

            if (item.indirectReferences.Count > 0)
            {
                EditorGUILayout.BeginVertical("Box");
                labelContent.text = "Indirect References";
                EditorGUILayout.LabelField(labelContent, SharedStyles.BuiltIn.centeredWrappedText);
                item.indirectExpanded = EditorGUI.Foldout(GUILayoutUtility.GetLastRect(), item.indirectExpanded, GUIContent.none, true);
                if (item.indirectExpanded)
                {
                    foreach (var aiRef in item.indirectReferences)
                    {
                        labelContent.text = aiRef.name;
                        EditorGUILayout.LabelField(labelContent);
                    }
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawAIListItem(AIReference item)
        {
            var labelContent = new GUIContent();

            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.BeginHorizontal(SharedStyles.BuiltIn.listItemHeader);
            labelContent.text = item.name;
            EditorGUILayout.LabelField(labelContent);
            EditorGUILayout.EndHorizontal();

            if (item.directReferences.Count > 0)
            {
                EditorGUILayout.BeginVertical("Box");
                labelContent.text = "Direct References";
                EditorGUILayout.LabelField(labelContent, SharedStyles.BuiltIn.centeredWrappedText);
                item.directReferencesExpanded = EditorGUI.Foldout(GUILayoutUtility.GetLastRect(), item.directReferencesExpanded, GUIContent.none, true);
                if (item.directReferencesExpanded)
                {
                    foreach (var aiRef in item.directReferences)
                    {
                        labelContent.text = aiRef.name;
                        EditorGUILayout.LabelField(labelContent);
                    }
                }

                EditorGUILayout.EndVertical();
            }

            if (item.directlyReferencingScenesOrPrefabs.Length > 0)
            {
                EditorGUILayout.BeginVertical("Box");
                labelContent.text = "Directly referenced by Scene or Prefab";
                EditorGUILayout.LabelField(labelContent, SharedStyles.BuiltIn.centeredWrappedText);
                item.directScenesOrPrefabsExpanded = EditorGUI.Foldout(GUILayoutUtility.GetLastRect(), item.directScenesOrPrefabsExpanded, GUIContent.none, true);
                if (item.directScenesOrPrefabsExpanded)
                {
                    foreach (var sof in item.directlyReferencingScenesOrPrefabs)
                    {
                        labelContent.text = sof.name;
                        EditorGUILayout.LabelField(labelContent);
                    }
                }

                EditorGUILayout.EndVertical();
            }

            if (item.directlyReferencingAIs.Length > 0)
            {
                EditorGUILayout.BeginVertical("Box");
                labelContent.text = "Directly referenced by AIs";
                EditorGUILayout.LabelField(labelContent, SharedStyles.BuiltIn.centeredWrappedText);
                item.directAIsExpanded = EditorGUI.Foldout(GUILayoutUtility.GetLastRect(), item.directAIsExpanded, GUIContent.none, true);
                if (item.directAIsExpanded)
                {
                    foreach (var aiRef in item.directlyReferencingAIs)
                    {
                        labelContent.text = aiRef.name;
                        EditorGUILayout.LabelField(labelContent);
                    }
                }

                EditorGUILayout.EndVertical();
            }

            if (item.indirectReferences.Count > 0)
            {
                EditorGUILayout.BeginVertical("Box");
                labelContent.text = "Indirect References";
                EditorGUILayout.LabelField(labelContent, SharedStyles.BuiltIn.centeredWrappedText);
                item.indirectReferencesExpanded = EditorGUI.Foldout(GUILayoutUtility.GetLastRect(), item.indirectReferencesExpanded, GUIContent.none, true);
                if (item.indirectReferencesExpanded)
                {
                    foreach (var aiRef in item.indirectReferences)
                    {
                        labelContent.text = aiRef.name;
                        EditorGUILayout.LabelField(labelContent);
                    }
                }

                EditorGUILayout.EndVertical();
            }

            if (item.indirectlyReferencingScenesOrPrefabs.Length > 0)
            {
                EditorGUILayout.BeginVertical("Box");
                labelContent.text = "Indirectly referenced by Scene or Prefab";
                EditorGUILayout.LabelField(labelContent, SharedStyles.BuiltIn.centeredWrappedText);
                item.indirectScenesOrPrefabsExpanded = EditorGUI.Foldout(GUILayoutUtility.GetLastRect(), item.indirectScenesOrPrefabsExpanded, GUIContent.none, true);
                if (item.indirectScenesOrPrefabsExpanded)
                {
                    foreach (var sof in item.indirectlyReferencingScenesOrPrefabs)
                    {
                        labelContent.text = sof.name;
                        EditorGUILayout.LabelField(labelContent);
                    }
                }

                EditorGUILayout.EndVertical();
            }

            if (item.indirectlyReferencingAIs.Length > 0)
            {
                EditorGUILayout.BeginVertical("Box");
                labelContent.text = "Indirectly referenced by AIs";
                EditorGUILayout.LabelField(labelContent, SharedStyles.BuiltIn.centeredWrappedText);
                item.indirectAIsExpanded = EditorGUI.Foldout(GUILayoutUtility.GetLastRect(), item.indirectAIsExpanded, GUIContent.none, true);
                if (item.indirectAIsExpanded)
                {
                    foreach (var aiRef in item.indirectlyReferencingAIs)
                    {
                        labelContent.text = aiRef.name;
                        EditorGUILayout.LabelField(labelContent);
                    }
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();
        }

        private IEnumerator<Action> ResolveReferences()
        {
            _aiReferences = new Dictionary<string, AIReference>(StringComparer.Ordinal);
            _referencers = new List<SceneOrPrefab>();

            //AIs
            var ais = StoredAIs.AIs.ToArray();
            for (int i = 0; i < ais.Length; i++)
            {
                if (_cancelled)
                {
                    yield break;
                }

                _progressMessage = string.Format("Processing AI {0}/{1}: {2}", i, ais.Length, ais[i].name);
                _owner.Repaint();

                yield return () =>
                {
                    var curAI = GetAIReference(ais[i]);
                    for (int j = 0; j < ais.Length; j++)
                    {
                        if (j == i)
                        {
                            continue;
                        }

                        if (ais[j].configuration.Contains(curAI.id))
                        {
                            var refedBy = GetAIReference(ais[j]);
                            refedBy.directReferences.Add(curAI);
                        }
                    }
                };
            }

            //Scenes
            var scenes = GetScenePaths(_scenes);
            if (scenes == null)
            {
                yield return () =>
                {
                    var references = GetSceneReferences();
                    CreateSceneOrPrefab(GetCurrentScene(), references, false);
                };
            }
            else
            {
                string curScene = GetCurrentScene();
                for (int i = 0; i < scenes.Length; i++)
                {
                    if (_cancelled)
                    {
                        yield break;
                    }

                    _progressMessage = string.Format("Processing scene {0}/{1}: {2}", i, scenes.Length, scenes[i]);
                    _owner.Repaint();
                    yield return () =>
                    {
                        OpenScene(scenes[i]);
                        var references = GetSceneReferences();
                        CreateSceneOrPrefab(scenes[i], references, false);
                    };
                }

                OpenScene(curScene);
            }

            //Prefabs
            if (_includePrefabs)
            {
                var prefabs = (from f in Directory.GetFiles(Application.dataPath, "*.prefab", SearchOption.AllDirectories)
                               select AssetPath.ProjectRelativePath(f)).ToArray();

                for (int i = 0; i < prefabs.Length; i++)
                {
                    if (_cancelled)
                    {
                        yield break;
                    }

                    _progressMessage = string.Format("Processing prefab {0}/{1}: {2}", i, prefabs.Length, prefabs[i]);
                    _owner.Repaint();

                    yield return () =>
                    {
                        var pf = AssetDatabase.LoadMainAssetAtPath(prefabs[i]) as GameObject;
                        if (pf != null)
                        {
                            var references = GetPrefabReferences(pf);
                            CreateSceneOrPrefab(prefabs[i], references, true);
                        }
                    };
                }
            }

            _progressMessage = "Cross referencing";
            _owner.Repaint();
            yield return () =>
            {
                PrepareAIResult();
            };
        }

        private AIReference GetAIReference(AIStorage ai)
        {
            AIReference aiRef;
            if (!_aiReferences.TryGetValue(ai.aiId, out aiRef))
            {
                aiRef = new AIReference(ai.aiId, ai.name);
                _aiReferences.Add(ai.aiId, aiRef);
            }

            return aiRef;
        }

        private void CreateSceneOrPrefab(string name, IEnumerable<string> refedIds, bool isPrefab)
        {
            var s = new SceneOrPrefab(name, isPrefab);

            AIReference aiRef;
            foreach (var aiId in refedIds)
            {
                if (_aiReferences.TryGetValue(aiId, out aiRef))
                {
                    s.directReferences.Add(aiRef);
                }
                else
                {
                    s.directReferences.Add(AIReference.missing);
                }
            }

            if (s.directReferences.Count > 0)
            {
                s.directReferences.Sort((a, b) => a.name.CompareTo(b.name));
                _referencers.Add(s);
            }
        }

        private IEnumerable<string> GetSceneReferences()
        {
            return (from item in Resources.FindObjectsOfTypeAll<UtilityAIComponent>()
                    from aiCfg in item.aiConfigs ?? Empty<UtilityAIConfig>.array
                    where !item.Equals(null) && PrefabUtility.GetPrefabType(item) != PrefabType.Prefab
                    select aiCfg.aiId).Distinct(StringComparer.Ordinal);
        }

        private IEnumerable<string> GetPrefabReferences(GameObject root)
        {
            var allGos = new List<GameObject>();
            root.SelfAndDescendants(allGos);

            return (from go in allGos
                    from item in go.GetComponents<UtilityAIComponent>()
                    from aiCfg in item.aiConfigs ?? Empty<UtilityAIConfig>.array
                    where !item.Equals(null) && PrefabUtility.GetPrefabType(item) == PrefabType.Prefab
                    select aiCfg.aiId).Distinct(StringComparer.Ordinal);
        }

        private string[] GetScenePaths(Scenes selection)
        {
            if (selection == Scenes.Current)
            {
                return null;
            }
            else if (selection == Scenes.Selected)
            {
                return this.sceneListView.GetSelectedItems();
            }

            return (from f in Directory.GetFiles(Application.dataPath, "*.unity", SearchOption.AllDirectories)
                    select AssetPath.ProjectRelativePath(f)).ToArray();
        }

        private void PrepareAIResult()
        {
            _referencers.Sort((a, b) => a.name.CompareTo(b.name));
            _aisList = _aiReferences.Values.OrderBy(ai => ai.name).ToArray();
            foreach (var aiRef in _aisList)
            {
                aiRef.recordScenesOrPrefabs(_referencers);
                aiRef.recordReferencingAIs(_aisList);
            }

            Array.Sort(
                _aisList,
                (a, b) =>
                {
                    var res = a.referencedByAny.CompareTo(b.referencedByAny);
                    if (res == 0)
                    {
                        return a.name.CompareTo(b.name);
                    }

                    return res;
                });
        }

        private string Export()
        {
            StringBuilder b = new StringBuilder();
            if (_aiGrouping == AIGrouping.ScenesAndPrefabs)
            {
                foreach (var item in _referencers.OrderBy(item => !item.isPrefab).ThenBy(item => item.name))
                {
                    b.AppendFormat("[{0}]", item.name);
                    b.AppendLine();
                    b.AppendLine();
                    b.AppendLine("Direct References:");
                    foreach (var aiRef in item.directReferences)
                    {
                        b.AppendLine(aiRef.name);
                    }

                    b.AppendLine();
                    if (item.indirectReferences.Count > 0)
                    {
                        b.AppendLine("Indirect References:");
                        foreach (var aiRef in item.indirectReferences)
                        {
                            b.AppendLine(aiRef.name);
                        }

                        b.AppendLine();
                    }
                }
            }
            else
            {
                foreach (var ai in _aisList)
                {
                    b.AppendFormat("[{0}]", ai.name);
                    b.AppendLine();
                    b.AppendLine();
                    if (ai.directReferences.Count > 0)
                    {
                        b.AppendLine("Direct References:");
                        foreach (var aiRef in ai.directReferences)
                        {
                            b.AppendLine(aiRef.name);
                        }

                        b.AppendLine();
                    }

                    if (ai.directlyReferencingScenesOrPrefabs.Length > 0)
                    {
                        b.AppendLine("Directly referenced by Scene or Prefab:");
                        foreach (var sof in ai.directlyReferencingScenesOrPrefabs)
                        {
                            b.AppendLine(sof.name);
                        }

                        b.AppendLine();
                    }

                    if (ai.directlyReferencingAIs.Length > 0)
                    {
                        b.AppendLine("Directly referenced by AIs:");
                        foreach (var aiRef in ai.directlyReferencingAIs)
                        {
                            b.AppendLine(aiRef.name);
                        }

                        b.AppendLine();
                    }

                    if (ai.indirectReferences.Count > 0)
                    {
                        b.AppendLine("Indirect References:");
                        foreach (var aiRef in ai.indirectReferences)
                        {
                            b.AppendLine(aiRef.name);
                        }

                        b.AppendLine();
                    }

                    if (ai.indirectlyReferencingScenesOrPrefabs.Length > 0)
                    {
                        b.AppendLine("Indirectly referenced by Scene or Prefab:");
                        foreach (var sof in ai.indirectlyReferencingScenesOrPrefabs)
                        {
                            b.AppendLine(sof.name);
                        }

                        b.AppendLine();
                    }

                    if (ai.indirectlyReferencingAIs.Length > 0)
                    {
                        b.AppendLine("Indirectly referenced by AIs:");
                        foreach (var aiRef in ai.indirectlyReferencingAIs)
                        {
                            b.AppendLine(aiRef.name);
                        }

                        b.AppendLine();
                    }
                }
            }

            return b.ToString();
        }

        private class AIReference
        {
            internal static readonly AIReference missing = new AIReference(null, "Missing");

            internal string id;
            internal string name;
            internal List<AIReference> directReferences;

            internal SceneOrPrefab[] directlyReferencingScenesOrPrefabs;
            internal SceneOrPrefab[] indirectlyReferencingScenesOrPrefabs;
            internal AIReference[] directlyReferencingAIs;
            internal AIReference[] indirectlyReferencingAIs;
            internal bool directReferencesExpanded = true;
            internal bool indirectReferencesExpanded;
            internal bool directScenesOrPrefabsExpanded = true;
            internal bool indirectScenesOrPrefabsExpanded;
            internal bool directAIsExpanded = true;
            internal bool indirectAIsExpanded;
            private List<AIReference> _indirectReferences;

            internal AIReference(string id, string name)
            {
                this.id = id;
                this.name = name;
                directReferences = new List<AIReference>();
            }

            internal bool referencedByAny
            {
                get
                {
                    return this.directlyReferencingAIs.Length > 0 || this.directlyReferencingScenesOrPrefabs.Length > 0;
                }
            }

            internal List<AIReference> indirectReferences
            {
                get
                {
                    if (_indirectReferences == null)
                    {
                        _indirectReferences = new List<AIReference>();
                        foreach (var aiRef in this.directReferences)
                        {
                            aiRef.GetReferences(_indirectReferences);
                        }

                        _indirectReferences.Sort((a, b) => a.name.CompareTo(b.name));
                    }

                    return _indirectReferences;
                }
            }

            internal void recordScenesOrPrefabs(IEnumerable<SceneOrPrefab> list)
            {
                directlyReferencingScenesOrPrefabs = (from sof in list
                                                      where sof.directReferences.Any(aiRef => aiRef == this)
                                                      select sof).ToArray();

                indirectlyReferencingScenesOrPrefabs = (from sof in list
                                                        where sof.indirectReferences.Any(aiRef => aiRef == this)
                                                        select sof).ToArray();
            }

            internal void recordReferencingAIs(IEnumerable<AIReference> list)
            {
                directlyReferencingAIs = (from ai in list
                                          where ai.directReferences.Any(aiRef => aiRef == this)
                                          select ai).ToArray();

                indirectlyReferencingAIs = (from ai in list
                                            where ai.indirectReferences.Any(aiRef => aiRef == this)
                                            select ai).ToArray();
            }

            private void GetReferences(List<AIReference> collector)
            {
                foreach (var aiRef in this.directReferences)
                {
                    if (collector.AddUnique(aiRef))
                    {
                        aiRef.GetReferences(collector);
                    }
                }
            }
        }

        private class SceneOrPrefab
        {
            internal string name;
            internal bool isPrefab;
            internal List<AIReference> directReferences;
            internal bool directExpanded = true;
            internal bool indirectExpanded;
            private List<AIReference> _indirectReferences;

            internal SceneOrPrefab(string name, bool isPrefab)
            {
                this.name = name;
                this.isPrefab = isPrefab;
                directReferences = new List<AIReference>();
            }

            internal List<AIReference> indirectReferences
            {
                get
                {
                    if (_indirectReferences == null)
                    {
                        _indirectReferences = (from aiRef in this.directReferences
                                               from indirectRef in aiRef.directReferences.Concat(aiRef.indirectReferences)
                                               select indirectRef).Distinct().OrderBy(ai => ai.name).ToList();
                    }

                    return _indirectReferences;
                }
            }
        }
    }
}