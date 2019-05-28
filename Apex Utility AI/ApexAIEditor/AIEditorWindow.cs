/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Apex.AI.Editor.UndoRedo;
    using Apex.AI.Visualization;
    using Apex.Editor;
    using UnityEditor;
    using UnityEngine;

    public class AIEditorWindow : EditorWindow
    {
        // various widths and heights in px
        private const float _saveAsWindowWidth = 220f;
        private const float _saveAsWindowHeight = 70f;
        private const float _minimumDragDelta = 25f;
        private const float _curveCurvature = 50f;

        public static AIEditorWindow activeInstance;

        private static readonly List<AIEditorWindow> _openWindows = new List<AIEditorWindow>();

        private static readonly Color _connectorLineColor = new Color(0f, 0f, 0f, 0.5f);
        private static readonly Color _connectorLineActiveColor = new Color(19f / 255f, 162f / 255f, 75f / 255f, 0.5f);
        private static readonly Color _gridBackground = new Color(42f / 255f, 42f / 255f, 42f / 255f);
        private static readonly Color _minorGridColor = new Color(36f / 255, 36f / 255, 36f / 255);
        private static readonly Color _majorGridColor = new Color(31f / 255, 31f / 255, 31f / 255);
        private static readonly Color _massSelectionColor = new Color(84f / 255f, 141f / 255f, 202f / 255f, 0.2f);

        private static GameObject _lastSelectedGameObject;

        private Material _GLMaterial;

        [SerializeField, HideInInspector]
        private string _lastOpenAI;

        [SerializeField, HideInInspector]
        private bool _destroyed;

        private AIUI _ui;
        private float _topPadding;
        private Texture2D _homeButtonTexture;
        private EditorStyling.ScaledStyles _scaledStyles;
        private ScaleSettings _scaling;

        // record key and mouse state
        private MouseState _mouse;
        private DragData _drag;
        private KeyCode _lastKey;
        private KeyCode[] _keysForcesRepaint = new KeyCode[] { KeyCode.LeftAlt, KeyCode.RightAlt, KeyCode.AltGr, KeyCode.LeftControl, KeyCode.RightControl, KeyCode.LeftCommand, KeyCode.RightCommand };

        // save as window "pop up"
        private SaveMode _saveMode;
        private string _saveAsName;
        private DateTime _lastSave;

        // state control
        private VisualizationMode _visualizationMode;
        private GameObject _visualizedEntity;
        private IQualifierVisualizer _breakpoint;
        private bool _playModePending;
        private bool _playModeEnded;
        private bool _wasPaused;
        private bool _focused;
        private bool _initialLoadComplete;

        //Gui content
        private GUIContent _title;
        private GUIContent _label;
        private GUIContent _labelWithTooltip;
        private GUIContent _icon;
        private GUIContent _iconWithTooltip;

        private enum SaveMode
        {
            None,
            SaveAs,
            SaveNew
        }

        private enum VisualizationMode
        {
            Default,
            Sticky,
            Linked
        }

        private static IEnumerable<AIEditorWindow> openWindows
        {
            get
            {
                int count = _openWindows.Count;
                for (int i = count - 1; i >= 0; i--)
                {
                    var window = _openWindows[i];
                    if (!window)
                    {
                        continue;
                    }

                    yield return window;
                }
            }
        }

        private Material GLMaterial
        {
            get
            {
                if (_GLMaterial == null || _GLMaterial.Equals(null))
                {
                    _GLMaterial = new Material(Shader.Find("Sprites/Default"));
                }

                return _GLMaterial;
            }
        }

        private float topPadding
        {
            get
            {
                if (_topPadding == 0f)
                {
                    _topPadding = EditorStyles.toolbar.fixedHeight;
                }

                return _topPadding;
            }
        }

        public static AIEditorWindow Open()
        {
            var win = CreateInstance<AIEditorWindow>();
            win.Show();
            win.SetTitle();
            return win;
        }

        [MenuItem("Assets/Create/Apex/Utility AI")]
        public static void UtilityAIMenu()
        {
            var win = Open();
            win.NewAI();
        }

        internal static void ToggleAutoSave(bool on)
        {
            Do(
               (win) =>
               {
                   if (win._ui != null)
                   {
                       EditorApplication.update -= win.OnEditorUpdate;

                       if (on)
                       {
                           EditorApplication.update += win.OnEditorUpdate;
                       }
                   }
               });
        }

        internal static void UpdateTitles()
        {
            Do(
                (win) =>
                {
                    win.SetTitle();
                    win.Repaint();
                });
        }

        internal static void Unload(params string[] aiIds)
        {
            Do(
                (win) =>
                {
                    if (aiIds.Contains(win._lastOpenAI, StringComparer.Ordinal))
                    {
                        win.Unload();
                    }
                });
        }

        internal static void Do(Action<AIEditorWindow> action)
        {
            openWindows.Apply(action);
        }

        internal static AIEditorWindow Find(string aiId)
        {
            return openWindows.FirstOrDefault(win => string.Equals(win._lastOpenAI, aiId, StringComparison.Ordinal));
        }

        internal static void UpdateVisualizedEntities()
        {
            if (!EditorApplication.isPlaying)
            {
                return;
            }

            //We reset all non sticky windows first since we need that done before updating since this must force an update of all ais.
            Do(w => w.ResetNonStickyEntity());

            Do(w => w.UpdateVisualizedEntity(true));
        }

        internal static void Open(string aiId)
        {
            // first check all available windows, if one of them has the correct AI open - just focus that window
            foreach (var window in openWindows)
            {
                var noneLoaded = (window._ui == null);
                if (noneLoaded || string.Equals(window._ui.ai.id.ToString(), aiId, StringComparison.Ordinal))
                {
                    window.Show();
                    window.Focus();
                    if (noneLoaded)
                    {
                        window.Load(aiId, true);
                    }

                    return;
                }
            }

            // if an open matching window could not be found - create a new one
            var win = Open();
            win.Load(aiId, true);
        }

        internal static void LoadInActive(Vector2 mousepos)
        {
            if (activeInstance == null)
            {
                return;
            }

            activeInstance.ShowLoadMenu(mousepos);
        }

        internal static void NewInActive()
        {
            if (activeInstance == null)
            {
                return;
            }

            activeInstance.NewAI();
        }

        internal void DeleteAI()
        {
            if (_ui.Delete())
            {
                _ui = null;
                _lastOpenAI = null;
                Selection.activeObject = null;
                SetTitle();
                Repaint();
            }
        }

        internal void Reload()
        {
            Load(_lastOpenAI, false);
        }

        private void NewAI()
        {
            EnsureChangesSaved();
            _ui = null;
            SetTitle();
            Selection.activeObject = null;
            _saveMode = SaveMode.SaveNew;
            Repaint();
        }

        private void ConfirmNewAI(string name)
        {
            _ui = AIUI.Create(name);
            _scaledStyles = EditorStyling.GetScaledStyles();
            _scaling = new ScaleSettings(_ui.canvas.zoom);

            _ui.AddSelector(new Vector2(100f, 100f), typeof(ScoreSelector), false);

            Save(name);
            SetTitle();
            Repaint();
        }

        private void Load(string aiId, bool forceRefresh)
        {
            if (string.IsNullOrEmpty(aiId))
            {
                _initialLoadComplete = EditorApplication.isPlaying;
                return;
            }

            bool refresh = forceRefresh;
            if (!refresh)
            {
                var state = Selection.activeObject as AIInspectorState;
                refresh = (state != null && !state.Equals(null) && state.currentAIUI != null && string.Equals(state.currentAIUI.ai.id.ToString(), aiId, StringComparison.Ordinal));
            }

            var tmp = AIUI.Load(aiId, refresh);

            //This indicates that the first load has been done, regardless of whether the AI exists or not.
            //We only do it when playing however as load at design time do not count since we use this to determine whether all AI windows have loaded to do visualization.
            _initialLoadComplete = EditorApplication.isPlaying;

            if (tmp == null)
            {
                Unload();
                return;
            }

            _lastOpenAI = aiId;
            _ui = tmp;
            _scaledStyles = EditorStyling.GetScaledStyles();
            _scaling = new ScaleSettings(_ui.canvas.zoom);
            SetTitle();

            if (EditorApplication.isPlaying && UserSettings.instance.visualDebug)
            {
                //Due to the dependencies of linked ais, i.e. to update a linked ai it must have been loaded, we wait with setting up visualization until all windows have loaded.
                if (openWindows.All(win => win._initialLoadComplete))
                {
                    if (VisualizationManager.BeginVisualization())
                    {
                        _lastSelectedGameObject = Selection.activeGameObject;

                        for (int i = 0; i < _openWindows.Count; i++)
                        {
                            _openWindows[i].UpdateVisualizedEntity(true);
                        }
                    }
                    else
                    {
                        //Loading after the initial load we still need to update all open windows due to potential linked AIS
                        UpdateVisualizedEntities();
                    }
                }
            }

            Repaint();
        }

        private void Unload()
        {
            _ui = null;
            _lastOpenAI = null;
            Selection.activeObject = null;
            SetTitle();
            Repaint();
        }

        private void OnEnable()
        {
            this.hideFlags = HideFlags.HideAndDontSave;

            if (_destroyed)
            {
                DestroyImmediate(this);
                return;
            }

            _lastSave = DateTime.Now;
            _mouse = new MouseState();
            _drag = new DragData(this);
            _scaling = ScaleSettings.FullScale;

            _title = new GUIContent();
            _label = new GUIContent();
            _labelWithTooltip = new GUIContent();
            _icon = new GUIContent();
            _iconWithTooltip = new GUIContent();

            _openWindows.Add(this);

            //Since EditorApplication.isPlaying or EditorApplication.isPlayingOrWillChangePlaymode is always false in OnEnable we can't use it to conditionally setup the window for play mode use.
            //Also since OnEnable is NOT called when stopping play mode, the window must be initialized the same way.
            //However the _playModePending variable keeps its value across state changes, so the things that we want to control differently in each state is only initialized once here, i.e. for edit mode,
            //the rest is done in the OnPlayStateChanged handler.
            EditorApplication.playmodeStateChanged += OnPlayStateChanged;

            //To ensure proper behaviour in all situations, e.g. opening while playing and while not,
            //we simply treat this as a play state change.
            _playModePending |= EditorApplication.isPlaying;
            _playModeEnded = !_playModePending;
            OnPlayStateChanged();

            //Due to the odd behaviour during recompiles (Enable being called AFTER Focus, plus see comment in OnFocus), we want to explicitly refresh the state in that scenario
            if (_focused && _ui != null)
            {
                _ui.RefreshState();
            }

            Repaint();
        }

        private void OnFocus()
        {
            //Since this is called on ALL windows after a recompile regardless of whether they are in fact focused,
            //we have to ensure that we only react to this if this is the actual focused window or no other window is active.
            if (EditorWindow.focusedWindow != this && AIEditorWindow.activeInstance != null)
            {
                return;
            }

            AIEditorWindow.activeInstance = this;
            _focused = true;

            if (_ui != null)
            {
                UpdateVisualizedEntity(false);
                _ui.RefreshState();
                _ui.PingAsset();
                this.Repaint();
            }
            else if (Selection.activeObject is AIInspectorState)
            {
                //We only reset active object if it is AI related, i.e. not if some game object is currently the selected item.
                //This is to prevent the case where AI is focused when play is started with a selected game object, which would then reset the selection
                Selection.activeObject = null;
            }
        }

        private void OnLostFocus()
        {
            _focused = false;

            if (_drag.isDragging)
            {
                var ct = _drag.CancelDrag();
                if (ct == ChangeTypes.None)
                {
                    return;
                }

                if (ct != ChangeTypes.Undoable)
                {
                    _ui.undoRedo.RegisterLayoutChange(ct);
                }

                _ui.isDirty = true;
                Repaint();
            }
        }

        private void OnDisable()
        {
            EditorApplication.playmodeStateChanged -= OnPlayStateChanged;
            EditorApplication.update -= OnEditorUpdate;
        }

        private void OnDestroy()
        {
            EnsureChangesSaved();
            _lastOpenAI = null;

            if (AIEditorWindow.activeInstance == this)
            {
                AIEditorWindow.activeInstance = null;
                Selection.activeObject = null;
            }

            _openWindows.Remove(this);
            _destroyed = true;
        }

        private void OnSelectionChange()
        {
            if (Selection.activeGameObject)
            {
                _lastSelectedGameObject = Selection.activeGameObject;
                UpdateVisualizedEntity(false);
            }
#if !UNITY_5 && !UNITY_2017
            //This is only for pre Unity 5.2
            if (VisualizationManager.isVisualizing)
            {
                VisualizationManager.UpdateSelectedGameObjects(Selection.gameObjects);
            }
#endif
        }

        private void SetTitle()
        {
            var title = (UserSettings.instance.showTabTitle && (_ui != null)) ? " " + _ui.name.Elipsify(12) : " Apex AI";
            this.SetTitle(title, UIResources.EditorWindowIcon.texture);
        }

        private void DoZoom(float scale, Vector2 mousePos)
        {
            if (_ui.Zoom(mousePos, scale, _scaling))
            {
                _scaledStyles.ScaleStyles(_scaling.scale);
                Repaint();
            }
        }

        private void UpdateVisualizedEntity(bool initialize)
        {
            if (!EditorApplication.isPlaying || _ui == null || !UserSettings.instance.visualDebug)
            {
                return;
            }

            var go = _lastSelectedGameObject;

            if (_visualizationMode == VisualizationMode.Sticky)
            {
                //If sticky we only update on startup. We also update linked ais when the selected go is the one stickied.
                if (_visualizedEntity == null)
                {
                    _visualizationMode = VisualizationMode.Default;
                    if (go == null)
                    {
                        return;
                    }
                }
                else if (initialize)
                {
                    go = _visualizedEntity;
                }
                else if (go != null && go.Equals(_visualizedEntity))
                {
                    UpdateLinkedAIs(_visualizedEntity, _ui.visualizedAI);
                    return;
                }
                else
                {
                    return;
                }
            }
            else if (go == null || go.Equals(_visualizedEntity))
            {
                return;
            }
            else
            {
                _visualizationMode = VisualizationMode.Default;
            }

            var client = AIManager.GetAIClient(go, _ui.ai.id);
            if (client != null)
            {
                SetVisualizedEntity(go, client.ai as UtilityAIVisualizer);
            }
            else if (_visualizationMode == VisualizationMode.Default)
            {
                SetVisualizedEntity(null, null);
            }
        }

        private void SetVisualizedEntity(GameObject visualizedEntity, UtilityAIVisualizer visualizedAI)
        {
            if (_ui.visualizedAI != null)
            {
                _ui.visualizedAI.Unhook(OnAIExecute);
            }

            if (visualizedEntity != null && visualizedAI != null)
            {
                _visualizedEntity = visualizedEntity;
                _ui.ShowVisualizedAI(visualizedAI);
                visualizedAI.Hook(OnAIExecute);

                UpdateLinkedAIs(visualizedEntity, visualizedAI);
            }
            else
            {
                _visualizedEntity = null;
                _ui.ShowVisualizedAI(null);
            }

            Repaint();
        }

        private void UpdateLinkedAIs(GameObject visualizedEntity, UtilityAIVisualizer visualizedAI)
        {
            if (visualizedAI == null)
            {
                return;
            }

            int linkedAIsCount = visualizedAI.linkedAIs.Count;
            if (linkedAIsCount > 0)
            {
                //If multiple clients link to the same ai, the one that is currently selected must overrule any others. Note these are game object so no ReferenceEquals!
                bool overrule = (_lastSelectedGameObject == _visualizedEntity);

                for (int i = 0; i < linkedAIsCount; i++)
                {
                    var linkedAi = visualizedAI.linkedAIs[i];
                    foreach (var window in openWindows)
                    {
                        if ((window._visualizationMode == VisualizationMode.Sticky) && (window._visualizedEntity != visualizedEntity))
                        {
                            continue;
                        }

                        var lastLoaded = window._lastOpenAI;
                        if (string.Equals(lastLoaded, linkedAi.id.ToString(), StringComparison.Ordinal))
                        {
                            window.SetLinkedVisualizedEntity(visualizedEntity, linkedAi, overrule);
                        }
                    }
                }
            }
        }

        private void SetLinkedVisualizedEntity(GameObject visualizedEntity, UtilityAIVisualizer linkVisualizer, bool overrule)
        {
            if (_visualizationMode == VisualizationMode.Default)
            {
                _visualizationMode = VisualizationMode.Linked;
            }

            //If we need to overrule or if not yet set, or if this is a sticky link
            if (overrule || _visualizedEntity == null || _visualizedEntity == visualizedEntity)
            {
                SetVisualizedEntity(visualizedEntity, linkVisualizer);
            }
        }

        private void ResetNonStickyEntity()
        {
            if (_visualizationMode != VisualizationMode.Sticky)
            {
                ResetVisualizedEntity();
            }
        }

        private void ResetVisualizedEntity()
        {
            _visualizedEntity = null;
            _visualizationMode = VisualizationMode.Default;

            if (_ui != null)
            {
                if (_ui.visualizedAI != null)
                {
                    _ui.visualizedAI.Unhook(OnAIExecute);
                }

                _ui.ShowVisualizedAI(null);
                this.Repaint();
            }
        }

        private void OnAIExecute()
        {
            this.Repaint();
        }

        private void OnPlayStateChanged()
        {
            if (!EditorApplication.isPlaying)
            {
                //Before entering play mode, ensure any changes are saved.
                //After returning from play mode, reestablish edit mode state, e.g. sign up for edit mode only events etc.
                if (EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    EnsureChangesSaved();
                    _playModePending = true;
                }
                else if (_playModeEnded)
                {
                    _playModeEnded = false;

                    if (UserSettings.instance.autoSaveDelay > 0)
                    {
                        EditorApplication.update += OnEditorUpdate;
                    }

                    if (_visualizationMode != VisualizationMode.Sticky)
                    {
                        _visualizedEntity = null;
                    }

                    Load(_lastOpenAI, false);
                }
            }
            else if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                if (_playModePending)
                {
                    //When entering play mode...
                    _playModePending = false;

                    Load(_lastOpenAI, false);
                }
                else if (EditorApplication.isPaused)
                {
                    _wasPaused = true;
                }
                else if (_wasPaused)
                {
                    _wasPaused = false;
                    if (_breakpoint != null)
                    {
                        _breakpoint.breakPointHit = false;
                        _breakpoint = null;
                    }
                }
            }
            else
            {
                _playModeEnded = true;
            }

            Repaint();
        }

        private void OnEditorUpdate()
        {
            if (EditorApplication.isPlaying)
            {
                return;
            }

            //Auto save
            if (_ui != null && _ui.isDirty)
            {
                int delay = UserSettings.instance.autoSaveDelay;

                if (EditorApplication.isCompiling || (delay > 0 && (DateTime.Now - _lastSave).Minutes > delay))
                {
                    _lastSave = DateTime.Now;
                    Save(null);
                    Repaint();
                }
            }
        }

        private void EnsureChangesSaved()
        {
            if (_ui == null || !_ui.isDirty)
            {
                return;
            }

            if (UserSettings.instance.promptToSave)
            {
                var msg = string.Format("There are {0} functional change(s) and {1} layout change(s).\n\nDo you wish to save?", _ui.undoRedo.functionalChangeCount, _ui.undoRedo.layoutChangeCount);
                if (!EditorUtility.DisplayDialog("Unsaved Changes", msg, "Yes", "No"))
                {
                    _ui.isDirty = false;
                    return;
                }
            }

            Save(null);
        }

        private void Save(string newName)
        {
            _ui.Save(newName);
            _lastOpenAI = _ui.ai.id.ToString();
        }

        private void OnGUI()
        {
            EditorStyling.InitScaleAgnosticStyles();

            var uiLoaded = (_ui != null);
            if (uiLoaded)
            {
                _scaledStyles.Initialize(_scaling.scale);
            }

            DrawBackgroundGrid();

            if (_saveMode != SaveMode.None)
            {
                if (uiLoaded)
                {
                    DrawUI();
                }

                DrawSaveAs();
            }
            else
            {
                if (uiLoaded)
                {
                    DrawUI();
                    OnPostGUI();
                }
                else
                {
                    ProcessMouseInputNoAI();
                }

                DrawToolbar();
            }
        }

        private GUIContent GetAITitle()
        {
            if (_ui.isDirty)
            {
                _title.text = string.Concat(_ui.name, "*");
                _title.tooltip = string.Format("{0} functional change(s), {1} layout change(s).", _ui.undoRedo.functionalChangeCount, _ui.undoRedo.layoutChangeCount);
            }
            else
            {
                _title.text = _ui.name;
                _title.tooltip = string.Empty;
            }

            return _title;
        }

        private GUIContent DoLabel(string text)
        {
            _label.text = text;
            return _label;
        }

        private GUIContent DoLabel(string text, string tooltip)
        {
            _labelWithTooltip.text = text;
            if (UserSettings.instance.showTooltips)
            {
                _labelWithTooltip.tooltip = tooltip;
            }
            else
            {
                _labelWithTooltip.tooltip = string.Empty;
            }

            return _labelWithTooltip;
        }

        private GUIContent DoIcon(Texture2D tex)
        {
            _icon.image = tex;
            return _icon;
        }

        private GUIContent DoIcon(Texture2D tex, string tooltip)
        {
            _iconWithTooltip.image = tex;
            _iconWithTooltip.tooltip = tooltip;
            return _iconWithTooltip;
        }

        private void ShowLoadMenu()
        {
            ShowLoadMenu(Event.current.mousePosition);
        }

        private void ShowLoadMenu(Vector2 mousepos)
        {
            EnsureChangesSaved();

            var screenPos = EditorGUIUtility.GUIToScreenPoint(mousepos);
            AISelectorWindow.Get(screenPos, (ai) => this.Load(ai.aiId, true));
        }

        private void DrawSaveAs()
        {
            var evt = Event.current;
            bool isEnter = (evt.keyCode == KeyCode.Return && evt.type == EventType.KeyDown);
            if (isEnter)
            {
                evt.Use();
            }

            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.FlexibleSpace();
            if (_ui != null)
            {
                GUIContent title = GetAITitle();
                GUILayout.Label(title, EditorStyling.Skinned.boldTitle);
                GUILayout.FlexibleSpace();
            }

            GUILayout.EndHorizontal();

            var saveAsWindowRect = new Rect(2f, this.topPadding + 2f, _saveAsWindowWidth, _saveAsWindowHeight);
            GUI.Box(saveAsWindowRect, string.Empty, EditorStyling.Canvas.normalSelector);

            saveAsWindowRect.x += 5f; // padding left
            saveAsWindowRect.width -= 10f; // padding right
            GUILayout.BeginArea(saveAsWindowRect);
            GUILayout.Space(2f); // padding top

            string header = (_saveMode == SaveMode.SaveAs) ? "Save as..." : "New AI name";
            GUILayout.Label(header, EditorStyling.Canvas.normalHeader, GUILayout.ExpandWidth(true));

            var labelContent = DoLabel("Name");
            EditorGUIUtility.labelWidth = GUI.skin.label.CalcSize(labelContent).x + 10f;

            GUI.SetNextControlName("SaveAs_SaveName");
            _saveAsName = EditorGUILayout.TextField(labelContent, _saveAsName, EditorStyles.textField);
            EditorGUI.FocusTextInControl("SaveAs_SaveName");

            EditorGUILayout.Separator();
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Ok", GUILayout.ExpandWidth(true)) || isEnter)
            {
                if (string.IsNullOrEmpty(_saveAsName))
                {
                    EditorUtility.DisplayDialog("Invalid Action", "You must assign a valid name for the AI", "Ok");
                    return;
                }
                else if (StoredAIs.NameExists(_saveAsName))
                {
                    EditorUtility.DisplayDialog("Duplicate Name", "An AI with the selected name already exists, you must select a unique name.", "Ok");
                    return;
                }

                if (_saveMode == SaveMode.SaveAs)
                {
                    Save(_saveAsName);
                }
                else if (_saveMode == SaveMode.SaveNew)
                {
                    ConfirmNewAI(_saveAsName);
                }

                _saveMode = SaveMode.None;
                _saveAsName = null;
                GUIUtility.keyboardControl = 0;
            }

            if (GUILayout.Button("Cancel", GUILayout.ExpandWidth(true)))
            {
                _saveAsName = null;
                _saveMode = SaveMode.None;
                GUIUtility.keyboardControl = 0;

                if (_lastOpenAI != null)
                {
                    Load(_lastOpenAI, false);
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void SetMouseCursor(MouseCursor cursor)
        {
            var viewPort = new Rect(0f, this.topPadding, this.position.width, this.position.height);

            EditorGUIUtility.AddCursorRect(viewPort, cursor);
        }

        private Rect GetViewport()
        {
            return new Rect
            {
                position = new Vector2(0f, this.topPadding),
                size = new Vector2(this.position.width, this.position.height - this.topPadding)
            };
        }

        private void DrawUI()
        {
            var viewCount = _ui.canvas.views.Count;
            if (viewCount == 0)
            {
                return;
            }

            var viewPort = GetViewport();

            if (!_drag.isDragging && Event.current.alt)
            {
                SetMouseCursor(MouseCursor.Pan);
            }

            // The order of draw methods dictate the drawing depth / layer
            DrawActionConnections(viewPort); // furthest from camera (occluded by others)
            DrawViews(viewPort);
            DrawHomeButton(); // closest to camera (overlaps others)
            DrawVisualizedEntity();
        }

        private void OnPostGUI()
        {
            var mousePos = Event.current.mousePosition;

            ProcessKeyInput();
            ProcessMouseInput(mousePos);
        }

        private void DrawViews(Rect viewPort)
        {
            //For drawing mouse cursor. Shift cannot be used here even though it can be used to do the selection, since for some reason shift is not sent as a separate keyboard event
            var evt = Event.current;
            bool multiSelect = (evt.control || evt.command);

            //Sort the views so selected are last, since we want them drawn topmost
            var views = _ui.canvas.views.OrderBy(v => v.isSelected);

            var iter = views.GetEnumerator();
            while (iter.MoveNext())
            {
                var view = iter.Current;
                view.RecalcHeight(_scaling);

                var viewRect = view.viewArea;
                if (!viewRect.Overlaps(viewPort))
                {
                    continue;
                }

                // draw the view
                ViewLayout baseLayout = null;
                if (view is SelectorView)
                {
                    var selectorView = (SelectorView)view;
                    var layout = new SelectorLayout(selectorView, this.topPadding, _scaling);
                    baseLayout = layout;
                    DrawSelectorUI(selectorView, layout);
                }
                else if (view is AILinkView)
                {
                    var linkView = (AILinkView)view;
                    var layout = new AILinkLayout(linkView, this.topPadding, _scaling);
                    baseLayout = layout;
                    DrawAILinkUI(linkView, layout);
                }

                // add resize cursor when over the 'resize margin areas'
                if (!_drag.isDragging)
                {
                    if (multiSelect)
                    {
                        EditorGUIUtility.AddCursorRect(viewRect, view.isSelected ? MouseCursor.ArrowMinus : MouseCursor.ArrowPlus);
                        continue;
                    }

                    if (baseLayout != null)
                    {
                        EditorGUIUtility.AddCursorRect(baseLayout.leftResizeArea, MouseCursor.ResizeHorizontal);
                        EditorGUIUtility.AddCursorRect(baseLayout.rightResizeArea, MouseCursor.ResizeHorizontal);
                    }
                }
            }
        }

        private void DrawAILinkUI(AILinkView linkView, AILinkLayout layout)
        {
            var viewRect = layout.viewRect;

            var isSelected = _focused && linkView.isSelected;
            var style = isSelected ? EditorStyling.Canvas.activeSelector : EditorStyling.Canvas.normalSelector;

            GUI.Box(viewRect, string.Empty, style);

            //Draw the header
            GUI.BeginGroup(viewRect);

            GUI.Label(new Rect(0f, 0f, viewRect.width, layout.titleHeight), linkView.title, isSelected ? _scaledStyles.viewTitleActive : _scaledStyles.viewTitle);

            //Draw the ai name
            GUI.Box(layout.GetContainerAreaLocal(), GUIContent.none, EditorStyling.Canvas.normalQualifier);

            GUI.Label(layout.GetNameAreaLocal(), DoLabel(linkView.aiName), _scaledStyles.normalBoxText);

            var showRect = layout.GetShowAreaLocal();
            EditorGUIUtility.AddCursorRect(showRect, MouseCursor.Link);
            GUI.Box(layout.GetIconArea(showRect), DoLabel(string.Empty, "Click to view linked AI."), EditorStyling.Canvas.viewButtonIcon);
            GUI.EndGroup();
        }

        private void DrawSelectorUI(SelectorView selectorView, SelectorLayout layout)
        {
            var selectorRect = selectorView.viewArea;
            var mousePos = Event.current.mousePosition;
            var titleHeight = layout.titleHeight;
            var width = selectorRect.width;
            var isSelected = _focused && selectorView.isSelected && object.ReferenceEquals(_ui.currentQualifier, null);

            GUIStyle selectorStyle;
            GUIStyle titleStyle;
            if (selectorView.isRoot)
            {
                selectorStyle = isSelected ? EditorStyling.Canvas.rootSelectorActive : EditorStyling.Canvas.rootSelectorNormal;
                titleStyle = isSelected ? _scaledStyles.rootViewTitleActive : _scaledStyles.rootViewTitle;
            }
            else
            {
                selectorStyle = isSelected ? EditorStyling.Canvas.activeSelector : EditorStyling.Canvas.normalSelector;
                titleStyle = isSelected ? _scaledStyles.viewTitleActive : _scaledStyles.viewTitle;
            }

            //Draw the header
            GUI.BeginGroup(selectorRect, selectorStyle);

            GUI.Label(new Rect(0f, 0f, width, titleHeight), selectorView.friendlyName, titleStyle);

            //Draw the qualifiers
            var y = titleHeight;
            var draggingQualifier = _drag.isDraggingQualifier(selectorView);

            // dropIndex is calculated as the mouse's y position relative to the height of one qualifier element, clamped between 0 and count - 1 (to avoid overflow)
            var dropIndex = -1;
            if (draggingQualifier)
            {
                var hit = layout.GetQualifierAtPosition(mousePos);
                dropIndex = hit.clampedIndex;
            }

            var idx = 0;
            var qualifierViews = selectorView.qualifierViews;
            var count = qualifierViews.Count;
            for (int i = 0; i < count; i++)
            {
                if (draggingQualifier && _drag.qualifierIdx == idx)
                {
                    // if loop is at the element we are dragging, iterate index
                    idx++;
                }

                // if mouse is over the 'current' iterated element, don't draw the element, instead leave a space
                if (i == dropIndex)
                {
                    y += qualifierViews[_drag.qualifierIdx].GetHeight(_scaling);
                }
                else
                {
                    var qualifierView = qualifierViews[idx++];
                    DrawCompleteQualifier(new Vector2(0f, y), width, qualifierView, layout);
                    y += qualifierView.GetHeight(_scaling);
                }
            }

            // default box
            DrawCompleteQualifier(new Vector2(0f, y), width, selectorView.defaultQualifierView, layout);

            // draw the qualifier that is currently being dragged
            if (draggingQualifier)
            {
                var pos = new Vector2(0f, mousePos.y - selectorRect.y - _drag.offset.y);
                DrawCompleteQualifier(pos, width, qualifierViews[_drag.qualifierIdx], layout);
            }

            GUI.EndGroup();
        }

        private void DrawCompleteQualifier(Vector2 position, float totalWidth, QualifierView qualifierView, SelectorLayout layout)
        {
            var x = position.x;
            var y = position.y;

            var visualizedQualifier = qualifierView.qualifier as IQualifierVisualizer;
            var visualize = _ui.isVisualizing;

            var actionView = qualifierView.actionView;
            var isActionSelected = _focused && actionView != null && object.ReferenceEquals(_ui.currentAction, actionView);
            var isQualifierSelected = !isActionSelected && _focused && object.ReferenceEquals(_ui.currentQualifier, qualifierView);
            var qualifierHeight = _scaling.qualifierHeight;
            var actionHeight = _scaling.actionHeight;

            GUIStyle qualifierStyle;
            GUIStyle actionStyle;
            if (visualize)
            {
                if (visualizedQualifier.isHighScorer)
                {
                    if (visualizedQualifier.breakPointHit && _breakpoint == null)
                    {
                        _breakpoint = visualizedQualifier;
                        EditorApplication.isPaused = true;
                    }

                    qualifierStyle = isQualifierSelected ? EditorStyling.Canvas.highScoreQualifierSelected : EditorStyling.Canvas.highScoreQualifier;
                    actionStyle = isActionSelected ? EditorStyling.Canvas.highScoreActionSelected : EditorStyling.Canvas.highScoreAction;
                }
                else
                {
                    qualifierStyle = isQualifierSelected ? EditorStyling.Canvas.lowScoreQualifierSelected : EditorStyling.Canvas.lowScoreQualifier;
                    actionStyle = isActionSelected ? EditorStyling.Canvas.lowScoreActionSelected : EditorStyling.Canvas.lowScoreAction;
                }
            }
            else
            {
                qualifierStyle = isQualifierSelected ? EditorStyling.Canvas.activeQualifier : EditorStyling.Canvas.normalQualifier;
                actionStyle = isActionSelected ? EditorStyling.Canvas.activeAction : EditorStyling.Canvas.normalAction;
            }

            GUI.Box(new Rect(x, y, totalWidth, qualifierView.GetHeight(_scaling) - _scaling.actionHeight), GUIContent.none, qualifierStyle);

            // dragable handle - don't draw for default qualifier or for non reorderable qualifier lists
            if (!qualifierView.isDefault && qualifierView.parent.reorderableQualifiers && !EditorApplication.isPlaying)
            {
                var dragRect = layout.GetDragAreaLocal(x, y);
                GUI.Label(dragRect, DoIcon(UIResources.DragHandle.texture, "Hold Left Mouse Down to Drag Qualifier"), EditorStyling.Canvas.smallButtonIcon);
                EditorGUIUtility.AddCursorRect(dragRect, MouseCursor.MoveArrow);
            }
            else if (visualize && visualizedQualifier.isBreakPoint)
            {
                var dragRect = layout.GetDragAreaLocal(x, y);
                var bptex = visualizedQualifier.breakPointHit ? UIResources.BreakPointHit.texture : UIResources.BreakPoint.texture;
                GUI.Box(dragRect, DoIcon(bptex), EditorStyling.Canvas.smallButtonIcon);
            }

            // qualifier
            bool isQualifierDisabled = qualifierView.qualifier.isDisabled;
            var qrect = layout.GetContentAreaLocal(x, y, qualifierHeight);
            var labelStyle = isQualifierDisabled ? (isQualifierSelected ? _scaledStyles.activeDisabledBoxText : _scaledStyles.disabledBoxText) : (isQualifierSelected ? _scaledStyles.activeBoxText : _scaledStyles.normalBoxText);
            GUI.Label(qrect, DoLabel(qualifierView.friendlyName, qualifierView.friendlyDescription), labelStyle);

            if (visualize)
            {
                if (visualizedQualifier is ICompositeVisualizer)
                {
                    var toggleRect = layout.GetToggleAreaLocal(x, y);
                    EditorGUIUtility.AddCursorRect(toggleRect, MouseCursor.Link);
                    if (qualifierView.isExpanded)
                    {
                        GUI.Box(toggleRect, DoIcon(UIResources.CollapseIcon.texture), EditorStyling.Canvas.smallButtonIcon);
                    }
                    else
                    {
                        GUI.Box(toggleRect, DoIcon(UIResources.ExpandIcon.texture), EditorStyling.Canvas.smallButtonIcon);
                    }
                }

                var scoreRect = layout.GetScoreAreaLocal(x, y, qualifierHeight);
                string scoreLiteral = visualizedQualifier.lastScore.HasValue ? visualizedQualifier.lastScore.Value.ToString("f0") : "-";
                GUI.Label(scoreRect, scoreLiteral, _scaledStyles.scoreText);
            }

            y += qualifierHeight;

            //Scorers
            if (qualifierView.isExpanded)
            {
                var scorerHeight = _scaling.scorerHeight;
                var cs = (ICompositeVisualizer)visualizedQualifier;
                var scorers = cs.children;
                var scorersCount = scorers.Count;
                if (scorersCount > 0)
                {
                    float adj = isQualifierSelected ? 1f : 0f;
                    GUI.Box(new Rect(x, y, totalWidth - adj, (scorerHeight * scorersCount) - adj), string.Empty, EditorStyling.Canvas.normalScorerBackground);

                    for (int i = 0; i < scorersCount; i++)
                    {
                        var scorer = (ScorerVisualizer)scorers[i];

                        var srect = layout.GetContentAreaLocal(x, y, scorerHeight);
                        GUI.Label(srect, DoLabel(DisplayHelper.GetFriendlyName(scorer.scorer.GetType())), _scaledStyles.normalScorer);

                        var scoreRect = layout.GetScoreAreaLocal(x, y, scorerHeight);
                        GUI.Label(scoreRect, scorer.lastScore, _scaledStyles.scoreText);

                        y += scorerHeight;
                    }
                }
            }

            // action
            GUI.Box(new Rect(x, y, totalWidth, actionHeight), GUIContent.none, actionStyle);

            bool connector = actionView == null || actionView is IConnectorActionView;
            bool showLabel = !connector || (actionView is CompositeActionView);
            if (connector)
            {
                var anchorRect = layout.GetAnchorAreaLocal(x, y);

                if (!EditorApplication.isPlaying)
                {
                    EditorGUIUtility.AddCursorRect(anchorRect, MouseCursor.Link);
                }

                var cav = actionView as CompositeActionView;

                if (actionView == null || (cav != null && cav.targetView == null))
                {
                    GUI.Box(anchorRect, DoIcon(UIResources.ConnectorOpen.texture, "Drag to Connect."), EditorStyling.Canvas.smallButtonIcon);
                }
                else
                {
                    bool visualizedSelected = visualize && visualizedQualifier.isHighScorer;
                    var tex = visualizedSelected ? UIResources.ConnectorActive.texture : UIResources.ConnectorUsed.texture;
                    GUI.Box(anchorRect, DoIcon(tex), EditorStyling.Canvas.smallButtonIcon);

                    var end = new Vector2(anchorRect.center.x + _curveCurvature, anchorRect.center.y);
                    DrawConnectionCurve(anchorRect.center, end, visualizedSelected);
                }
            }

            if (showLabel)
            {
                var iconWidth = UIResources.ActionMarker.width;
                var abaserect = layout.GetContentAreaLocal(x, y, actionHeight);

                var iconRect = new Rect(abaserect.xMin, y, iconWidth, actionHeight);
                GUI.Box(iconRect, DoIcon(UIResources.ActionMarker.texture), EditorStyling.Canvas.smallButtonIcon);

                var arect = new Rect(iconRect.xMax, y, abaserect.width - iconWidth, actionHeight);
                labelStyle = isQualifierDisabled ? (isActionSelected ? _scaledStyles.activeDisabledBoxText : _scaledStyles.disabledBoxText) : (isActionSelected ? _scaledStyles.activeBoxText : _scaledStyles.normalBoxText);
                GUI.Label(arect, DoLabel(actionView.friendlyName, actionView.friendlyDescription), labelStyle);
            }
        }

        private void DrawHomeButton()
        {
            var cellSize = _scaling.snapCellSize;
            var panVector = GetPanToOriginVector();
            if (panVector.sqrMagnitude < (cellSize * cellSize))
            {
                return;
            }

            var rect = new Rect(this.position.width - 35f, this.topPadding + 10f, 30f, 20f);
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

            if (_homeButtonTexture == null)
            {
                _homeButtonTexture = UIResources.HomeIcon.texture;
            }

            if (GUI.Button(rect, DoIcon(_homeButtonTexture, "Click to re-center canvas ('Home' key)"), EditorStyles.label))
            {
                Pan(panVector);
            }
        }

        private void DrawVisualizedEntity()
        {
            if (!EditorApplication.isPlaying || _visualizedEntity == null)
            {
                return;
            }

            var layout = new VisualizedEntityLayout(this.position, this.topPadding);

            GUI.Box(layout.containerArea, GUIContent.none, EditorStyling.Canvas.visualizedEntityContainer);
            EditorGUIUtility.AddCursorRect(layout.containerArea, MouseCursor.Link);

            GUI.Label(layout.nameArea, DoLabel(_visualizedEntity.name), EditorStyling.Canvas.normalHeader);

            var stickyIcon = _visualizationMode == VisualizationMode.Sticky ? DoIcon(UIResources.PinOnIcon.texture) : DoIcon(UIResources.PinOffIcon.texture);
            GUI.Box(layout.stickyArea, stickyIcon, EditorStyling.Canvas.smallButtonIcon);

            GUI.Box(layout.resetArea, DoIcon(UIResources.CancelIcon.texture), EditorStyling.Canvas.smallButtonIcon);
        }

        private void DrawActionConnections(Rect viewPort)
        {
            foreach (var selectorView in _ui.canvas.selectorViews)
            {
                bool isDraggingQualifier = _drag.isDraggingQualifier(selectorView);
                var selectorRect = selectorView.viewArea;

                var qIdx = -1;
                foreach (var qualifierView in selectorView.AllQualifierViews)
                {
                    qIdx++;

                    var av = qualifierView.actionView as IConnectorActionView;
                    if (av == null)
                    {
                        continue;
                    }

                    var connectedView = av.targetView;
                    if (connectedView == null)
                    {
                        continue;
                    }

                    var connectedRect = connectedView.viewArea;
                    if (!selectorRect.Overlaps(viewPort) && !connectedRect.Overlaps(viewPort))
                    {
                        continue;
                    }

                    var startPos = selectorView.ConnectorAnchorOut(qIdx, _scaling);
                    if (isDraggingQualifier && _drag.qualifierIdx == qIdx)
                    {
                        var mousePos = Event.current.mousePosition;
                        startPos.y = mousePos.y + (qualifierView.GetHeight(_scaling) - (_scaling.actionHeight * 0.5f)) - _drag.offset.y;
                    }

                    var endPos = connectedView.ConnectorAnchorIn(_scaling);

                    DrawConnectionCurve(startPos, endPos, qualifierView.isHighScorer);
                }
            }
        }

        private void DrawBackgroundGrid()
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            var width = this.position.width;
            var height = this.position.height;
            var size = _scaling.snapCellSize;
            var offset = (_ui != null) ? _ui.canvas.offset : Vector2.zero;

            //Draw the background
            GLMaterial.SetPass(0);

            GL.PushMatrix();
            GL.Begin(GL.QUADS);

            GL.Color(_gridBackground);

            GL.Vertex(new Vector3(0f, 0f, 0f));
            GL.Vertex(new Vector3(width, 0f, 0f));
            GL.Vertex(new Vector3(width, height, 0f));
            GL.Vertex(new Vector3(0f, height, 0f));

            GL.End();
            GL.PopMatrix();

            if (!UserSettings.instance.showGrid)
            {
                return;
            }

            //Draw the lines
            GLMaterial.SetPass(0);

            GL.PushMatrix();
            GL.Begin(GL.LINES);

            GL.Color(_minorGridColor);
            DrawGridLines(offset, width, height, size);

            GL.Color(_majorGridColor);
            DrawGridLines(offset, width, height, size * 10f);

            GL.End();
            GL.PopMatrix();
        }

        private void DrawGridLines(Vector2 offset, float width, float height, float size)
        {
            var deltaX = offset.x % size;
            var deltaY = (offset.y % size);

            deltaX = ((deltaX < 0f) ? deltaX + size : deltaX);
            deltaY = ((deltaY < 0f) ? deltaY + size : deltaY);

            for (float x = deltaX; x < width; x += size)
            {
                GL.Vertex(new Vector2(x, 0f));
                GL.Vertex(new Vector2(x, height));
            }

            for (float y = deltaY; y < height; y += size)
            {
                GL.Vertex(new Vector2(0f, y));
                GL.Vertex(new Vector2(width, y));
            }
        }

        private void DrawToolbar()
        {
            if (Event.current.isKey)
            {
                //We do not want to do this on key events as it messes with the editor's rendering.
                return;
            }

            var isPlaying = EditorApplication.isPlaying;

            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (!isPlaying)
            {
                if (GUILayout.Button("New", EditorStyles.toolbarButton))
                {
                    NewAI();
                }
            }

            if (StoredAIs.AIs.count > 0)
            {
                EditorGUILayout.Separator();
                if (GUILayout.Button("Load", EditorStyles.toolbarButton))
                {
                    ShowLoadMenu();
                }
            }

            if (_ui != null)
            {
                if (_ui.rootSelector != null)
                {
                    EditorGUILayout.Separator();
                    if (GUILayout.Button("Save", EditorStyles.toolbarButton))
                    {
                        Save(null);
                    }

                    if (!isPlaying)
                    {
                        EditorGUILayout.Separator();
                        if (GUILayout.Button("Save As...", EditorStyles.toolbarButton))
                        {
                            _saveMode = SaveMode.SaveAs;
                        }
                    }
                }

                GUILayout.FlexibleSpace();
                GUIContent title = GetAITitle();
                GUILayout.Label(title, EditorStyling.Skinned.boldTitle);
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button(DoIcon(UIResources.ZoomIcon.texture, "Zoom, click to reset."), EditorStyles.label))
            {
                var viewPort = GetViewport();
                DoZoom(1f, viewPort.center);
            }

            var settings = UserSettings.instance;
            var newScale = GUILayout.HorizontalSlider(_scaling.scale, settings.zoomMin, settings.zoomMax, GUILayout.Width(100f));
            if (newScale != _scaling.scale)
            {
                var viewPort = GetViewport();
                DoZoom(newScale, viewPort.center);
            }

            GUILayout.Space(4f);
            var sg = GUILayout.Toggle(UserSettings.instance.showGrid, DoIcon(UIResources.ShowGridIcon.texture, "Toggle Grid"), EditorStyles.toolbarButton);
            if (sg != UserSettings.instance.showGrid)
            {
                UserSettings.instance.showGrid = sg;
            }

            if (isPlaying)
            {
                var vd = GUILayout.Toggle(UserSettings.instance.visualDebug, "Visual Debugging", EditorStyles.toolbarButton);
                if (vd != UserSettings.instance.visualDebug)
                {
                    UserSettings.instance.visualDebug = vd;

                    if (vd)
                    {
                        VisualizationManager.BeginVisualization();

                        Do(
                            w =>
                            {
                                w.UpdateVisualizedEntity(false);
                                w.Repaint();
                            });
                    }
                    else
                    {
                        Do(
                            w =>
                            {
                                w.CollapsAllQualifiers();
                                w.ResetVisualizedEntity();
                            });
                    }
                }
            }

            GUILayout.EndHorizontal();
        }

        private void CollapsAllQualifiers()
        {
            if (_ui == null)
            {
                return;
            }

            foreach (var sv in _ui.canvas.selectorViews)
            {
                foreach (var qv in sv.qualifierViews)
                {
                    qv.isExpanded = false;
                }
            }
        }

        private bool forceRepaint(KeyCode key)
        {
            for (int i = 0; i < _keysForcesRepaint.Length; i++)
            {
                if (key == _keysForcesRepaint[i])
                {
                    return true;
                }
            }

            return false;
        }

        private void ProcessKeyInput()
        {
            var evt = Event.current;

            if (!evt.isKey)
            {
                return;
            }

            if (HandleKeyPanning())
            {
                return;
            }

            if (evt.type == EventType.KeyDown)
            {
                if (evt.keyCode != _lastKey && forceRepaint(evt.keyCode))
                {
                    _lastKey = evt.keyCode;
                    Repaint();
                }

                return;
            }
            else if (evt.type != EventType.KeyUp)
            {
                return;
            }

            //While only really necessary for events that trigger pop-ups, we just cancel drags on any key to make it easy.
            if (_drag.isDragging)
            {
                var ct = _drag.CancelDrag();
                if (ct != ChangeTypes.None)
                {
                    if (ct != ChangeTypes.Undoable)
                    {
                        _ui.undoRedo.RegisterLayoutChange(ct);
                    }

                    _ui.isDirty = true;
                    Repaint();
                }
            }

            if (_lastKey != KeyCode.None && _lastKey == evt.keyCode)
            {
                _lastKey = KeyCode.None;
                Repaint();
            }

            if (evt.keyCode == KeyCode.Home)
            {
                Pan(GetPanToOriginVector());
                return;
            }

            if (Application.isPlaying)
            {
                // Only allow panning and pan-to-origin (home key) when in Play mode
                return;
            }

            if (evt.keyCode == KeyCode.Delete)
            {
                _ui.RemoveSelected();
                return;
            }

            if (evt.control || evt.command) // Control (WIN) or command (OSX)
            {
                if (evt.keyCode == KeyCode.C)
                {
                    // CTRL/CMD + C = Copy
                    ClipboardService.CopyToClipboard(_ui);
                }
                else if (evt.keyCode == KeyCode.X)
                {
                    // CTRL/CMD + X = Cut
                    ClipboardService.CutToClipboard(_ui);
                    Repaint();
                }
                else if (evt.keyCode == KeyCode.D)
                {
                    // CTRL/CMD + D = Duplicate/Clone
                    ClipboardService.Duplicate(_ui, evt.mousePosition);
                    Repaint();
                }
                else if (evt.keyCode == KeyCode.V)
                {
                    // CTRL/CMD + V = Paste
                    ClipboardService.PasteFromClipboard(_ui, evt.mousePosition);
                    Repaint();
                }
                else if (evt.keyCode == KeyCode.A)
                {
                    // CTRL/CMD + A = Select All
                    _ui.MultiSelectViews(_ui.canvas.views);
                    Repaint();
                }
                else if (evt.keyCode == KeyCode.R || evt.keyCode == KeyCode.F5)
                {
                    // CTRL/CMD + F5 = Refresh
                    // CTRL/CMD + R = Refresh (Repaint)
                    Debug.Log(string.Concat("APEX AI - ", _ui.name, " - REFRESHED"));
                    AssetDatabase.Refresh();
                    Repaint();
                }
                else if (evt.keyCode == KeyCode.L)
                {
                    // CTRL/CMD + L = Load
                    ShowLoadMenu();
                }

                if (evt.shift)
                {
                    if (evt.keyCode == KeyCode.Z)
                    {
                        // CTRL/CMD + Shift + Z = Undo
                        _ui.undoRedo.Undo();
                        Repaint();
                    }
                    else if (evt.keyCode == KeyCode.Y)
                    {
                        // CTRL/CMD + Shift + Y = Redo
                        _ui.undoRedo.Redo();
                        Repaint();
                    }
                }
            }
        }

        private bool HandleKeyPanning()
        {
            var evt = Event.current;

            var keySpeed = UserSettings.instance.keyPanSpeed;
            if (evt.shift)
            {
                keySpeed *= 2f;
            }

            var keyMovement = Vector2.zero;
            if (evt.keyCode == KeyCode.UpArrow || evt.keyCode == KeyCode.Keypad8)
            {
                // UP KEY
                keyMovement.y -= keySpeed;
            }
            else if (evt.keyCode == KeyCode.DownArrow || evt.keyCode == KeyCode.Keypad2)
            {
                // DOWN KEY
                keyMovement.y += keySpeed;
            }

            if (evt.keyCode == KeyCode.RightArrow || evt.keyCode == KeyCode.Keypad6)
            {
                // RIGHT KEY
                keyMovement.x += keySpeed;
            }
            else if (evt.keyCode == KeyCode.LeftArrow || evt.keyCode == KeyCode.Keypad4)
            {
                // LEFT KEY
                keyMovement.x -= keySpeed;
            }

            if (keyMovement.sqrMagnitude > 0f)
            {
                if (evt.type == EventType.KeyDown)
                {
                    PanInternal(keyMovement);
                }
                else if (evt.type == EventType.KeyUp)
                {
                    _ui.undoRedo.RegisterLayoutChange(ChangeTypes.Pan);
                    _ui.isDirty = true;
                }

                Repaint();
                return true;
            }

            return false;
        }

        private void ProcessMouseInputNoAI()
        {
            var evt = Event.current;
            _mouse.Update(evt);

            var mousePos = evt.mousePosition;
            if (!evt.isMouse || mousePos.y < this.topPadding)
            {
                return;
            }

            if (_mouse.isMouseUp && _mouse.isRightButton)
            {
                AIEditorMenus.ShowBlankCanvasMenu(mousePos);
            }
        }

        private void ProcessMouseInput(Vector2 mousePos)
        {
            var evt = Event.current;
            _mouse.Update(evt);

            //Process end states first. As long as we are in some sort of drag operation there is no more to do before it ends.
            if (_drag.isDragging)
            {
                if ((_mouse.isMouseUp && (_mouse.isLeftButton || _mouse.isMiddleButton)) || evt.type == EventType.Ignore || evt.type == EventType.Used)
                {
                    evt.Use();
                    var ct = _drag.EndDrag(mousePos);
                    if (ct != ChangeTypes.None)
                    {
                        if (ct != ChangeTypes.Undoable)
                        {
                            _ui.undoRedo.RegisterLayoutChange(ct);
                        }

                        _ui.isDirty = true;
                        Repaint();
                    }
                }
                else if (_mouse.isMouseDrag)
                {
                    Repaint();
                }
                else if (evt.type == EventType.Repaint)
                {
                    _drag.DoDrag(mousePos);
                }

                return;
            }

            if (_mouse.isMouseWheel)
            {
                evt.Use();
                var zoom = _scaling.scale - (UserSettings.instance.zoomSpeed * evt.delta.y);
                DoZoom(zoom, mousePos);
                return;
            }

            //Since this handler includes the toolbar, we need to ensure that drags are properly contained.
            //We don't want a drag initiated on the toolbar to start drawing on the canvas, but we do want the drag to happen, e.g. zoom slider.
            if (_mouse.isMouseDown)
            {
                _drag.lastMouseDownOnCanvas = (mousePos.y >= this.topPadding);
            }
            else if (_mouse.isMouseDrag && !_drag.lastMouseDownOnCanvas)
            {
                return;
            }

            if (!evt.isMouse || !(_mouse.isLeftButton || _mouse.isRightButton || _mouse.isMiddleButton) || mousePos.y < this.topPadding)
            {
                return;
            }

            evt.Use();
            var isPlaying = EditorApplication.isPlaying;

            if (isPlaying && _visualizedEntity != null && _mouse.isLeftButton && _mouse.isMouseDown)
            {
                var visualizationView = new VisualizedEntityLayout(this.position, this.topPadding);
                var hit = visualizationView.GetHitInfo(mousePos);
                if (hit != null)
                {
                    if (hit.inNameArea)
                    {
                        Selection.activeGameObject = _visualizedEntity;
                    }
                    else if (hit.inStickyArea)
                    {
                        _visualizationMode = (_visualizationMode == VisualizationMode.Sticky) ? VisualizationMode.Default : VisualizationMode.Sticky;
                    }
                    else if (hit.inResetArea)
                    {
                        ResetVisualizedEntity();
                    }

                    return;
                }
            }

            //Reset keyboard focus when on canvas, e.g. buttons or sliders need to loose input focus or they will respond to events
            if (GUIUtility.hotControl != 0 && _mouse.isMouseDown)
            {
                GUIUtility.hotControl = 0;
            }

            var view = _ui.canvas.ViewAtPosition(mousePos);

            //Pan overrides all
            if ((evt.alt || _mouse.isMiddleButton) && _mouse.isMouseDown)
            {
                _drag.StartPan(mousePos);
                return;
            }

            //Canvas actions
            if (view == null)
            {
                if (_mouse.isMouseDown)
                {
                    if (_mouse.isLeftButton)
                    {
                        _ui.ResetSelections();
                    }
                }
                else if (_mouse.isMouseUp)
                {
                    if (_mouse.isRightButton)
                    {
                        if (isPlaying)
                        {
                            AIEditorMenus.ShowRuntimeViewMenu(_ui, mousePos);
                        }
                        else
                        {
                            AIEditorMenus.ShowCanvasMenu(_ui, mousePos);
                        }
                    }
                }
                else if (_mouse.isMouseDrag)
                {
                    if (_mouse.isLeftButton)
                    {
                        _drag.StartMassSelect(mousePos);
                    }
                }

                return;
            }

            //Layout resolution
            ViewLayout layout;
            var selectorView = view as SelectorView;
            var linkView = view as AILinkView;

            if (selectorView != null)
            {
                layout = new SelectorLayout(selectorView, this.topPadding, _scaling);
            }
            else
            {
                layout = new AILinkLayout(linkView, this.topPadding, _scaling);
            }

            //Common view actions
            if (_mouse.isMouseDown)
            {
                if (evt.control || evt.command || evt.shift)
                {
                    _ui.MultiSelectView(view);
                    return;
                }

                if (_ui.selectedViews.Count > 1)
                {
                    //On multi select there is nothing more to do in mouse down
                    return;
                }

                if (_mouse.isLeftButton)
                {
                    if (layout.InResizeArea(mousePos))
                    {
                        _drag.StartResize(view, mousePos);
                        return;
                    }
                    else if (layout.InTitleArea(mousePos))
                    {
                        _drag.StartViewDrag(view, mousePos);
                    }
                }
            }
            else if (_mouse.isMouseDrag)
            {
                if (_ui.selectedViews.Count > 1)
                {
                    //do drag views and return
                    _drag.StartViewDrag(view, mousePos);
                    return;
                }
            }

            //Selector, Qualifiers and Actions
            QualifierView selectedQualifier = null;
            ActionView selectedAction = null;

            if (selectorView != null && _mouse.isMouseDown)
            {
                var selectorLayout = (SelectorLayout)layout;
                var qualifierHit = selectorLayout.GetQualifierAtPosition(mousePos);

                selectedQualifier = qualifierHit.qualifier;
                if (qualifierHit.InActionArea && !(selectedQualifier.actionView is SelectorActionView))
                {
                    selectedAction = selectedQualifier.actionView;
                }

                if (_mouse.isLeftButton)
                {
                    if (!isPlaying)
                    {
                        if (qualifierHit.InDragArea)
                        {
                            _drag.StartQualifierDrag(selectorLayout, mousePos);
                        }
                        else if (qualifierHit.InAnchorArea)
                        {
                            _drag.StartConnectorDrag(selectorLayout, mousePos);
                        }
                    }
                    else if (qualifierHit.InToggle)
                    {
                        selectedQualifier.isExpanded = !selectedQualifier.isExpanded;
                    }
                }

                _ui.Select(selectorView, selectedQualifier, selectedAction);
            }
            else if (linkView != null && _mouse.isMouseDown)
            {
                var linkLayout = (AILinkLayout)layout;
                if (linkLayout.InShowArea(mousePos))
                {
                    var linkedId = linkView.aiId.ToString();
                    if (StoredAIs.GetById(linkedId) != null)
                    {
                        Open(linkedId);
                    }
                    else
                    {
                        linkView.Refresh();
                    }
                }

                _ui.Select(linkView);
            }
            else if (_mouse.isMouseUp)
            {
                if (_mouse.isRightButton)
                {
                    if (isPlaying)
                    {
                        AIEditorMenus.ShowRuntimeViewMenu(_ui, mousePos);
                    }
                    else
                    {
                        AIEditorMenus.ShowViewMenu(_ui, mousePos);
                    }
                }
                else if (_ui.selectedViews.Count > 1 && !(evt.control || evt.command || evt.shift))
                {
                    if (selectorView != null)
                    {
                        _ui.Select(selectorView, selectedQualifier, selectedAction);
                    }
                    else if (linkView != null)
                    {
                        _ui.Select(linkView);
                    }
                }
            }
        }

        private void DrawConnectionCurve(Vector3 startPos, Vector3 endPos, bool visualizedSelected)
        {
            var startTan = startPos + (Vector3.right * _curveCurvature);
            var endTan = endPos + (Vector3.left * _curveCurvature);

            var texture = UIResources.ConnectorLine.texture;
            var lineColor = visualizedSelected ? _connectorLineActiveColor : _connectorLineColor;
            Handles.DrawBezier(startPos, endPos, startTan, endTan, lineColor, texture, _scaling.connectorLineWidth);
        }

        private void Pan(Vector2 pan)
        {
            if (pan.sqrMagnitude == 0f || _ui.canvas.views.Count == 0)
            {
                return;
            }

            PanInternal(pan);
            _ui.undoRedo.RegisterLayoutChange(ChangeTypes.Pan);
            _ui.isDirty = true;

            Repaint();
        }

        private Vector2 GetPanToOriginVector()
        {
            var rootView = _ui.canvas.views.SingleOrDefault(sv => (sv is SelectorView) && ((SelectorView)sv).isRoot);
            var rootRect = rootView != null ? rootView.viewArea : new Rect();
            var centerPos = new Vector2((this.position.width - rootRect.width) * 0.5f, (this.position.height - rootRect.height) * 0.5f);
            return centerPos - rootRect.position;
        }

        private void PanInternal(Vector2 pan)
        {
            pan = pan.Round();
            var views = _ui.canvas.views;
            var viewCount = views.Count;
            for (int i = 0; i < viewCount; i++)
            {
                views[i].viewArea.position += pan;
            }

            _ui.canvas.offset += pan;
        }

        private Rect SnapToGrid(Rect value)
        {
            return _ui.canvas.SnapToGrid(value);
        }

        internal void SnapAllToGrid()
        {
            if (_ui == null)
            {
                return;
            }

            if (_ui.canvas.SnapAllToGrid() > 0)
            {
                _ui.undoRedo.RegisterLayoutChange(ChangeTypes.Move);
                _ui.isDirty = true;
            }

            Repaint();
        }

        private class DragData
        {
            public bool lastMouseDownOnCanvas;

            private AIEditorWindow _parent;
            private DragType _type;
            private TopLevelView _view;
            private SelectorView _selector;
            private QualifierView _qualifier;
            private SelectorLayout _layout;
            private bool _resizeLeft;
            private int _qualifierIdx;
            private Rect _startPositionAndSize;
            private Vector2 _offset;
            private Vector2 _dragStart;
            private Vector2 _dragLast;
            private Vector3 _dragAnchor;

            public DragData(AIEditorWindow parent)
            {
                _parent = parent;
            }

            public enum DragType
            {
                None,
                View,
                Qualifier,
                Connector,
                Resize,
                MassSelect,
                Pan
            }

            public bool isDragging
            {
                get { return _type != DragType.None; }
            }

            public int qualifierIdx
            {
                get { return _qualifierIdx; }
            }

            public Vector2 dragStart
            {
                get { return _dragStart; }
            }

            public Vector2 offset
            {
                get { return _offset; }
            }

            public Vector3 anchor
            {
                get { return _dragAnchor; }
            }

            public bool isDraggingType(DragType t)
            {
                return t == _type;
            }

            public bool isDraggingQualifier(SelectorView sv)
            {
                return (_type == DragType.Qualifier) && object.ReferenceEquals(sv, _selector);
            }

            public void StartViewDrag(TopLevelView target, Vector2 mousePos)
            {
                _type = DragType.View;
                _view = target;
                _dragStart = _dragLast = mousePos;
            }

            public void StartQualifierDrag(SelectorLayout layout, Vector2 mousePos)
            {
                if (!layout.selectorView.reorderableQualifiers)
                {
                    return;
                }

                var hit = layout.GetQualifierAtPosition(mousePos);

                _qualifierIdx = hit.index;
                if (_qualifierIdx < 0 || _qualifierIdx >= layout.selectorView.qualifierViews.Count)
                {
                    return;
                }

                _type = DragType.Qualifier;
                _layout = layout;
                _selector = layout.selectorView;
                _offset = hit.offset;
            }

            public void StartConnectorDrag(SelectorLayout layout, Vector2 mousePos)
            {
                var hit = layout.GetQualifierAtPosition(mousePos);

                _qualifier = hit.qualifier;
                if (_qualifier.actionView != null && !(_qualifier.actionView is IConnectorActionView))
                {
                    return;
                }

                _dragStart = mousePos;
                _type = DragType.Connector;
                _qualifierIdx = hit.index;
                _dragAnchor = layout.selectorView.ConnectorAnchorOut(_qualifierIdx, _parent._scaling);
                _dragAnchor.x -= (_parent._scaling.selectorResizeMargin + (_parent._scaling.anchorAreaWidth * 0.5f));
            }

            public void StartMassSelect(Vector2 mousePos)
            {
                _dragStart = mousePos;
                _type = DragType.MassSelect;
            }

            public void StartResize(TopLevelView target, Vector2 mousePos)
            {
                _view = target;
                _dragStart = mousePos;
                _startPositionAndSize = target.viewArea;
                _type = DragType.Resize;
                _resizeLeft = mousePos.x < target.viewArea.center.x;
            }

            public void StartPan(Vector2 mousePos)
            {
                _dragStart = _dragLast = mousePos;
                _type = DragType.Pan;
            }

            public void DoDrag(Vector2 mousePos)
            {
                switch (_type)
                {
                    case DragType.View:
                    {
                        DoViewDrag(mousePos);
                        break;
                    }

                    case DragType.Qualifier:
                    {
                        _parent.SetMouseCursor(MouseCursor.MoveArrow);
                        break;
                    }

                    case DragType.Resize:
                    {
                        DoResize(mousePos);
                        break;
                    }

                    case DragType.Connector:
                    {
                        DoConnectorDrag(mousePos);
                        break;
                    }

                    case DragType.MassSelect:
                    {
                        DoMassSelect(mousePos);
                        break;
                    }

                    case DragType.Pan:
                    {
                        DoPan(mousePos);
                        break;
                    }
                }
            }

            private void DoViewDrag(Vector2 mousePos)
            {
                var delta = mousePos - _dragLast;
                _dragLast = mousePos;

                var selectedViews = _parent._ui.selectedViews;
                var count = selectedViews.Count;
                for (int i = 0; i < count; i++)
                {
                    selectedViews[i].viewArea.position += delta;
                }
            }

            private void DoConnectorDrag(Vector2 mousePos)
            {
                // draw connection line to mouse pos
                var dragDelta = (_dragStart - mousePos).sqrMagnitude;
                if (dragDelta > (_minimumDragDelta * _minimumDragDelta))
                {
                    _parent.DrawConnectionCurve(_dragAnchor, new Vector3(mousePos.x, mousePos.y, 0f), false);
                }
            }

            private void DoMassSelect(Vector2 mousePos)
            {
                _parent.GLMaterial.SetPass(0);

                GL.PushMatrix();
                GL.Begin(GL.QUADS);

                GL.Color(_massSelectionColor);

                GL.Vertex(new Vector3(_dragStart.x, _dragStart.y, 0f));
                GL.Vertex(new Vector3(mousePos.x, _dragStart.y, 0f));
                GL.Vertex(new Vector3(mousePos.x, mousePos.y, 0f));
                GL.Vertex(new Vector3(_dragStart.x, mousePos.y, 0f));

                GL.End();
                GL.PopMatrix();
            }

            private void DoResize(Vector2 mousePos)
            {
                var resizeRect = _view.viewArea;
                if (_resizeLeft)
                {
                    // left-side resize (move x, increase width)
                    var diff = resizeRect.x - mousePos.x;
                    if (resizeRect.width + diff > _parent._scaling.viewMinWidth)
                    {
                        _view.viewArea.x = mousePos.x;
                        _view.viewArea.width += diff;
                    }
                }
                else
                {
                    // right-side resize (increase width)
                    var diff = mousePos.x - resizeRect.x;
                    if (diff > _parent._scaling.viewMinWidth)
                    {
                        _view.viewArea.width = diff;
                    }
                }

                _parent.SetMouseCursor(MouseCursor.ResizeHorizontal);
            }

            private void DoPan(Vector2 mousePos)
            {
                var delta = mousePos - _dragLast;
                _dragLast = mousePos;
                _parent.PanInternal(delta * UserSettings.instance.canvasPanSensitivity);

                _parent.SetMouseCursor(MouseCursor.Pan);
            }

            public ChangeTypes EndDrag(Vector2 mousePos)
            {
                var change = ChangeTypes.None;

                switch (_type)
                {
                    case DragType.View:
                    {
                        change = EndViewDrag(mousePos);
                        break;
                    }

                    case DragType.Qualifier:
                    {
                        change = EndQualifierDrag(mousePos);
                        break;
                    }

                    case DragType.Connector:
                    {
                        change = EndDragConnector(mousePos);
                        break;
                    }

                    case DragType.Resize:
                    {
                        change = EndResize(mousePos);
                        break;
                    }

                    case DragType.MassSelect:
                    {
                        change = EndMassSelect(mousePos);
                        break;
                    }

                    case DragType.Pan:
                    {
                        change = EndPan(mousePos);
                        break;
                    }
                }

                _type = DragType.None;
                _view = null;
                _selector = null;
                _qualifier = null;
                _layout = null;
                return change;
            }

            public ChangeTypes CancelDrag()
            {
                var change = ChangeTypes.None;

                switch (_type)
                {
                    case DragType.View:
                    {
                        var selectedSelectors = _parent._ui.selectedViews;
                        foreach (var s in selectedSelectors)
                        {
                            s.viewArea = _parent.SnapToGrid(s.viewArea);
                        }

                        change = (_selector.viewArea != _startPositionAndSize) ? ChangeTypes.Move : ChangeTypes.None;
                        break;
                    }

                    case DragType.Resize:
                    {
                        _selector.viewArea = _parent.SnapToGrid(_selector.viewArea);
                        change = (_selector.viewArea != _startPositionAndSize) ? ChangeTypes.Resize : ChangeTypes.None;
                        break;
                    }
                }

                _type = DragType.None;
                _view = null;
                _selector = null;
                _qualifier = null;
                _layout = null;

                return change;
            }

            private ChangeTypes EndViewDrag(Vector2 mousePos)
            {
                if ((mousePos - _dragStart).sqrMagnitude < 1f)
                {
                    return ChangeTypes.None;
                }

                var selectedViews = _parent._ui.selectedViews;
                foreach (var s in selectedViews)
                {
                    s.viewArea = _parent.SnapToGrid(s.viewArea);
                }

                return (_view.viewArea != _startPositionAndSize) ? ChangeTypes.Move : ChangeTypes.None;
            }

            private ChangeTypes EndQualifierDrag(Vector2 mousePos)
            {
                var hit = _layout.GetQualifierAtPosition(mousePos);
                var dropIndex = hit.clampedIndex;

                if (dropIndex != _qualifierIdx)
                {
                    // no need to reorder if dropping the qualifier at the position where it was originally
                    _selector.selector.qualifiers.Reorder(_qualifierIdx, dropIndex);
                    _selector.qualifierViews.Reorder(_qualifierIdx, dropIndex);
                    _parent._ui.undoRedo.Do(new ReorderOperation(_qualifierIdx, dropIndex, _selector.selector.qualifiers, _selector.qualifierViews));
                    return ChangeTypes.Undoable;
                }

                return ChangeTypes.None;
            }

            private ChangeTypes EndDragConnector(Vector2 mousePos)
            {
                var target = _parent._ui.canvas.ViewAtPosition(mousePos);

                // Clear and remove selector action on "failed drag" (i.e. when the drag does not end on top of a selector rect)
                if (target == null)
                {
                    if (_qualifier.actionView is CompositeActionView)
                    {
                        _parent._ui.ResetConnection((CompositeActionView)_qualifier.actionView);
                        return ChangeTypes.Undoable;
                    }

                    if (_qualifier.actionView is IConnectorActionView)
                    {
                        _parent._ui.RemoveAction(_qualifier.actionView);
                        return ChangeTypes.Undoable;
                    }
                }
                else
                {
                    var sv = _qualifier.parent;
                    if (!object.ReferenceEquals(sv, target))
                    {
                        return _parent._ui.Connect(_qualifier, target) ? ChangeTypes.Undoable : ChangeTypes.None;
                    }
                }

                return ChangeTypes.None;
            }

            private ChangeTypes EndMassSelect(Vector2 mousePos)
            {
                var selectArea = new Rect(_dragStart.x, _dragStart.y, mousePos.x - _dragStart.x, mousePos.y - _dragStart.y);
                var selection = _parent._ui.canvas.views.Where(s => selectArea.Overlaps(s.viewArea, true));
                _parent._ui.MultiSelectViews(selection);

                //selections should not count as a change
                return ChangeTypes.None;
            }

            private ChangeTypes EndResize(Vector2 mousePos)
            {
                _view.viewArea = _parent.SnapToGrid(_view.viewArea);
                return (_view.viewArea != _startPositionAndSize) ? ChangeTypes.Resize : ChangeTypes.None;
            }

            private ChangeTypes EndPan(Vector2 mousePos)
            {
                return ChangeTypes.Pan;
            }
        }
    }
}