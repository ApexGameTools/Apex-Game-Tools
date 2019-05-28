/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Editor.Versioning
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Apex.Editor;
    using UnityEditor;
#if UNITY_2017
    using UnityEditor.SceneManagement;
#endif
    using UnityEngine;
    using Utilities;

    public class VersionUpgraderWindow : EditorWindow
    {
        private static readonly string[] _scenesTabTitles = new string[] { "Current Scene", "Selected Scenes", "All Scenes" };

        private string _progressMessage;
        private bool _cancelled;
        private GUIContent _listItemContent = new GUIContent();
        private GUIContent _tooltippedLabel = new GUIContent();

        private ListView<string> _sceneSelectView;
        private IEnumerable<string> _sceneList;
        private bool _includePrefabs = true;
        private Step _step;
        private Scenes _scenes = Scenes.All;

        private Vector2 _scrollPos;
        private VersionUpgradeAction[] _allActions;
        private OptionalAction[] _optionalActions;
        private List<string> _failedScenes;
        private int _processedScenes;
        private int _changedScenes;
        private int _scenesTotal;

        private enum Step
        {
            NoUpdates,
            Start,
            SelectUpdates,
            Processing,
            Result
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
                    _sceneSelectView = new ListView<string>(this, RenderListItem, MatchItem, false, true, (ignored) => StartUpdate());
                    _sceneList = GetScenePaths(Scenes.All);
                }

                return _sceneSelectView;
            }
        }

        public static void ShowWindow()
        {
            EditorWindow.GetWindow<VersionUpgraderWindow>(true, "Apex Version Updater");
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

        private static void SaveScene()
        {
#if UNITY_2017
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
#else
            EditorApplication.SaveScene();
#endif
        }

        private void OnEnable()
        {
            this.minSize = new Vector2(550f, 495f);

            _allActions = (from a in AppDomain.CurrentDomain.GetAssemblies()
                           where a.GetCustomAttributes(typeof(ApexProductAttribute), true).Any()
                           from t in a.GetTypes()
                           where t.IsSubclassOf(typeof(VersionUpgradeAction)) && !t.IsAbstract
                           let act = Activator.CreateInstance(t) as VersionUpgradeAction
                           orderby act.priority descending
                           select act).ToArray();

            if (_allActions.Length == 0)
            {
                _step = Step.NoUpdates;
            }
            else
            {
                _step = Step.Start;
                _optionalActions = (from a in _allActions
                                    where a.isOptional
                                    orderby a.name
                                    select new OptionalAction
                                    {
                                        action = a,
                                        selected = false
                                    }).ToArray();
            }
        }

        private void OnDisable()
        {
            _scenes = Scenes.All;
            _sceneSelectView = null;
            _step = Step.Start;
        }

        private void OnGUI()
        {
            switch (_step)
            {
                case Step.NoUpdates:
                {
                    DrawNoUpdates();
                    break;
                }

                case Step.Start:
                {
                    DrawStart();
                    break;
                }

                case Step.SelectUpdates:
                {
                    DrawSelectUpdates();
                    break;
                }

                case Step.Processing:
                {
                    DrawWorking();
                    break;
                }

                case Step.Result:
                {
                    DrawComplete();
                    break;
                }
            }
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

        private void DrawNoUpdates()
        {
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("No updating is required.", SharedStyles.BuiltIn.wrappedText);
            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Close"))
            {
                this.Close();
            }
        }

        private void DrawStart()
        {
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("This tool will update components and their settings to account for changes made between versions of Apex products.", SharedStyles.BuiltIn.wrappedText);
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Select which scene(s) to update.", SharedStyles.BuiltIn.centeredText);
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
                EditorGUILayout.LabelField("Fixed Selection.", SharedStyles.BuiltIn.centeredText);
                GUILayout.FlexibleSpace();
            }

            if (_optionalActions.Length == 0)
            {
                if (GUILayout.Button("Update"))
                {
                    StartUpdate();
                }
            }
            else
            {
                if (GUILayout.Button("Next"))
                {
                    _step = Step.SelectUpdates;
                    Repaint();
                }
            }
        }

        private void DrawSelectUpdates()
        {
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Select optional updates.", SharedStyles.BuiltIn.centeredText);
            EditorGUILayout.EndVertical();

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            for (int i = 0; i < _optionalActions.Length; i++)
            {
                var oa = _optionalActions[i];
                oa.selected = EditorGUILayout.ToggleLeft(oa.action.name, oa.selected, GUILayout.Height(30f));
            }

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Update"))
            {
                StartUpdate();
            }
        }

        private void StartUpdate()
        {
#if UNITY_2017
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
#else
            EditorApplication.SaveCurrentSceneIfUserWantsTo();
#endif

            _step = Step.Processing;
            EditorAsync.ExecuteOnMain(
                ExecuteUpdates(),
                10,
                () =>
                {
                    if (_cancelled)
                    {
                        _cancelled = false;
                    }

                    _step = Step.Result;

                    Repaint();
                });

            Repaint();
        }

        private IEnumerator<Action> ExecuteUpdates()
        {
            var actions = _allActions.Where(a => !a.isOptional).Concat(_optionalActions.Where(oa => oa.selected).Select(oa => oa.action)).OrderByDescending(a => a.priority).ToArray();

            if (_includePrefabs)
            {
                _progressMessage = "Updating prefabs";
                Repaint();

                var prefabs = from f in Directory.GetFiles(Application.dataPath, "*.prefab", SearchOption.AllDirectories)
                              select AssetPath.ProjectRelativePath(f);

                yield return () =>
                {
                    var loadedPrefabs = new List<UnityEngine.Object>();
                    foreach (var prefab in prefabs)
                    {
                        if (_cancelled)
                        {
                            return;
                        }

                        loadedPrefabs.Add(AssetDatabase.LoadMainAssetAtPath(prefab));
                    }

                    foreach (var act in actions)
                    {
                        act.Upgrade();
                    }
                };
            }

            //Next scenes
            string curScene = GetCurrentScene();
            var scenes = GetScenePaths(_scenes);

            _failedScenes = new List<string>();
            _scenesTotal = scenes.Length;

            for (int i = 0; i < scenes.Length; i++)
            {
                if (_cancelled)
                {
                    yield break;
                }

                _progressMessage = string.Format("Updating scene {0}/{1}: {2}", i, scenes.Length, scenes[i]);
                Repaint();

                yield return () =>
                {
                    if (OpenScene(scenes[i]))
                    {
                        bool changed = false;
                        foreach (var act in actions)
                        {
                            changed |= act.Upgrade();
                        }

                        if (changed)
                        {
                            _changedScenes++;
                            SaveScene();
                        }

                        _processedScenes++;
                    }
                    else
                    {
                        _failedScenes.Add(scenes[i]);
                    }
                };
            }

            OpenScene(curScene);
            AssetDatabase.SaveAssets();
            Resources.UnloadUnusedAssets();
        }

        private void DrawWorking()
        {
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Working on it....", SharedStyles.BuiltIn.centeredText);
            EditorGUILayout.LabelField(_progressMessage, SharedStyles.BuiltIn.centeredText);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Cancel"))
            {
                _cancelled = true;
            }
        }

        private void DrawComplete()
        {
            StringBuilder message = new StringBuilder();

            if (_includePrefabs)
            {
                message.AppendLine("All prefabs were successfully updated to the latest version of installed Apex Products.");
                message.AppendLine();
            }

            if (_scenesTotal > 0)
            {
                message.AppendFormat("{0}/{1} scenes were successfully updated to the latest version of installed Apex Products ({2} actually changed).", _processedScenes, _scenesTotal, _changedScenes);
                message.AppendLine();
                message.AppendLine();
            }

            if (_failedScenes.Count > 0)
            {
                message.AppendLine("The following scenes could not be loaded for update:");
                foreach (var fs in _failedScenes)
                {
                    message.AppendLine(fs);
                }

                message.AppendLine();
                message.AppendLine();
            }

            if (message.Length > 0)
            {
                message.Append("Please note that all prefab instances that had modified properties have been reset to the values of their prefab (Only applies to Apex Components).");
            }
            else
            {
                message.Append("No updates were made.");
            }

            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField(message.ToString(), SharedStyles.BuiltIn.wrappedText);
            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Close"))
            {
                this.Close();
            }
        }

        private string[] GetScenePaths(Scenes selection)
        {
            if (selection == Scenes.Current)
            {
                if (string.IsNullOrEmpty(GetCurrentScene()))
                {
                    return Empty<string>.array;
                }

                return new string[] { GetCurrentScene() };
            }
            else if (selection == Scenes.Selected)
            {
                return this.sceneListView.GetSelectedItems();
            }

            return (from f in Directory.GetFiles(Application.dataPath, "*.unity", SearchOption.AllDirectories)
                    select AssetPath.ProjectRelativePath(f)).ToArray();
        }

        private class OptionalAction
        {
            internal VersionUpgradeAction action;
            internal bool selected;
        }
    }
}