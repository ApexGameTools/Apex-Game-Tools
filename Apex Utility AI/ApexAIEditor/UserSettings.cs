/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Editor
{
    using UnityEditor;
    using UnityEngine;

    public sealed class UserSettings
    {
        private const string qualifierHeightKey = "ApexAI_QualifierHeight";
        private const string titleHeightKey = "ApexAI_titleHeightKey";
        private const string actionHeightKey = "ApexAI_ActionHeight";
        private const string scorerHeightKey = "ApexAI_ScorerHeight";
        private const string canvasPanSensitivityKey = "ApexAI_CanvasPanSensitivity";
        private const string snapCellSizeKey = "ApexAI_SnapCellSize";
        private const string showGridKey = "ApexAI_ShowGrid";
        private const string showTabTitleKey = "ApexAI_ShowTabTitle";
        private const string keyPanSpeedKey = "ApexAI_KeyPanSpeed";
        private const string showTooltipsKey = "ApexAI_ShowTooltips";
        private const string confirmDeletesKey = "ApexAI_ConfirmDeletes";
        private const string promptToSaveKey = "ApexAI_PromptToSaveKey";
        private const string autoSaveDelayKey = "ApexAI_AutoSaveDelay";
        private const string pingAssetKey = "ApexAI_PingAsset";
        private const string visualDebugKey = "ApexAI_VisualDebug";
        private const string autoGenerateNameMapKey = "ApexAI_AutoGenerateNameMap";
        private const string maxUndoKey = "ApexAI_MaxUndo";
        private const string zoomSpeedKey = "ApexAI_ZoomSpeed";
        private const string zoomMinKey = "ApexAI_MinZoomKey";
        private const string zoomMaxKey = "ApexAI_MaxZoomKey";
        private const string skinKey = "ApexAI_SkinKey";

        private float _qualifierHeight;
        private float _titleHeight;
        private float _actionHeight;
        private float _scorerHeight;
        private float _canvasPanSensitivity;
        private float _snapCellSize;
        private float _keyPanSpeed;
        private float _zoomSpeed;
        private float _zoomMin;
        private float _zoomMax;
        private int _autoSaveDelay;
        private int _maxUndo;
        private bool _pingAsset;
        private bool _showTooltips;
        private bool _confirmDeletes;
        private bool _promptToSave;
        private bool _visualDebug;
        private bool _autoGenerateNameMap;
        private bool _showGrid;
        private bool _showTabTitle;
        private string _skin;

        public static readonly UserSettings instance = CreateInstance();

        public string skin
        {
            get
            {
                return _skin;
            }

            set
            {
                if (Skins.options.IndexOf(value) >= 0)
                {
                    _skin = value;
                    EditorPrefs.SetString(skinKey, _skin);
                    ResourceManager.Reset();
                    EditorStyling.Canvas.Init();
                    EditorStyling.Canvas.SetTextures();
                    AIEditorWindow.Do(win => win.Repaint());
                }
            }
        }

        public float qualifierHeight
        {
            get
            {
                return _qualifierHeight;
            }

            set
            {
                _qualifierHeight = Mathf.Round(Mathf.Max(value, 20f));
                EditorPrefs.SetFloat(qualifierHeightKey, _qualifierHeight);
                AIEditorWindow.Do(win => win.Repaint());
            }
        }

        public float titleHeight
        {
            get
            {
                return _titleHeight;
            }

            set
            {
                _titleHeight = Mathf.Round(Mathf.Max(value, 20f));
                EditorPrefs.SetFloat(titleHeightKey, _titleHeight);
                AIEditorWindow.Do(win => win.Repaint());
            }
        }

        public float actionHeight
        {
            get
            {
                return _actionHeight;
            }

            set
            {
                _actionHeight = Mathf.Round(Mathf.Max(value, 20f));
                EditorPrefs.SetFloat(actionHeightKey, _actionHeight);
                AIEditorWindow.Do(win => win.Repaint());
            }
        }

        public float scorerHeight
        {
            get
            {
                return _scorerHeight;
            }

            set
            {
                _scorerHeight = Mathf.Round(Mathf.Max(value, 20f));
                EditorPrefs.SetFloat(scorerHeightKey, _scorerHeight);
                AIEditorWindow.Do(win => win.Repaint());
            }
        }

        public float canvasPanSensitivity
        {
            get
            {
                return _canvasPanSensitivity;
            }

            set
            {
                _canvasPanSensitivity = Mathf.Max(value, 0.5f);
                EditorPrefs.SetFloat(canvasPanSensitivityKey, _canvasPanSensitivity);
            }
        }

        public float snapCellSize
        {
            get
            {
                return _snapCellSize;
            }

            set
            {
                _snapCellSize = Mathf.Round(Mathf.Max(value, 10f));
                EditorPrefs.SetFloat(snapCellSizeKey, _snapCellSize);
                AIEditorWindow.Do(w => w.SnapAllToGrid());
            }
        }

        public float keyPanSpeed
        {
            get
            {
                return _keyPanSpeed;
            }

            set
            {
                _keyPanSpeed = Mathf.Max(value, 1f);
                EditorPrefs.SetFloat(keyPanSpeedKey, _keyPanSpeed);
            }
        }

        public float zoomSpeed
        {
            get
            {
                return _zoomSpeed;
            }

            set
            {
                _zoomSpeed = Mathf.Max(value, 0.001f);
                EditorPrefs.SetFloat(zoomSpeedKey, _zoomSpeed);
            }
        }

        public float zoomMin
        {
            get
            {
                return _zoomMin;
            }

            set
            {
                _zoomMin = Mathf.Min(Mathf.Max(value, 0.1f), 1f);
                EditorPrefs.SetFloat(zoomMinKey, _zoomMin);
            }
        }

        public float zoomMax
        {
            get
            {
                return _zoomMax;
            }

            set
            {
                _zoomMax = Mathf.Max(value, 1f);
                EditorPrefs.SetFloat(zoomMaxKey, _zoomMax);
            }
        }

        public int autoSaveDelay
        {
            get
            {
                return _autoSaveDelay;
            }

            set
            {
                _autoSaveDelay = value;
                EditorPrefs.SetInt(autoSaveDelayKey, value);
                AIEditorWindow.ToggleAutoSave(value > 0);
            }
        }

        public int maxUndo
        {
            get
            {
                return _maxUndo;
            }

            set
            {
                _maxUndo = Mathf.Max(value, 5);
                EditorPrefs.SetInt(maxUndoKey, _maxUndo);
            }
        }

        public bool showTooltips
        {
            get
            {
                return _showTooltips;
            }

            set
            {
                _showTooltips = value;
                EditorPrefs.SetBool(showTooltipsKey, value);
            }
        }

        public bool confirmDeletes
        {
            get
            {
                return _confirmDeletes;
            }

            set
            {
                _confirmDeletes = value;
                EditorPrefs.SetBool(confirmDeletesKey, value);
            }
        }

        public bool promptToSave
        {
            get
            {
                return _promptToSave;
            }

            set
            {
                _promptToSave = value;
                EditorPrefs.SetBool(promptToSaveKey, value);
            }
        }

        public bool pingAsset
        {
            get
            {
                return _pingAsset;
            }

            set
            {
                _pingAsset = value;
                EditorPrefs.SetBool(pingAssetKey, value);
            }
        }

        public bool visualDebug
        {
            get
            {
                return _visualDebug;
            }

            set
            {
                _visualDebug = value;
                EditorPrefs.SetBool(visualDebugKey, value);
            }
        }

        public bool autoGenerateNameMap
        {
            get
            {
                return _autoGenerateNameMap;
            }

            set
            {
                _autoGenerateNameMap = value;
                EditorPrefs.SetBool(autoGenerateNameMapKey, value);
            }
        }

        public bool showGrid
        {
            get
            {
                return _showGrid;
            }

            set
            {
                _showGrid = value;
                EditorPrefs.SetBool(showGridKey, value);
            }
        }

        public bool showTabTitle
        {
            get
            {
                return _showTabTitle;
            }

            set
            {
                _showTabTitle = value;
                EditorPrefs.SetBool(showTabTitleKey, value);
                AIEditorWindow.UpdateTitles();
            }
        }

        private static UserSettings CreateInstance()
        {
            var s = new UserSettings();
            s.Init();

            return s;
        }

        private void Init()
        {
            _skin = EditorPrefs.GetString(skinKey, "default");
            _qualifierHeight = EditorPrefs.GetFloat(qualifierHeightKey, 35f);
            _titleHeight = EditorPrefs.GetFloat(titleHeightKey, 40f);
            _actionHeight = EditorPrefs.GetFloat(actionHeightKey, 25f);
            _scorerHeight = EditorPrefs.GetFloat(scorerHeightKey, 25f);
            _canvasPanSensitivity = EditorPrefs.GetFloat(canvasPanSensitivityKey, 1f);
            _snapCellSize = EditorPrefs.GetFloat(snapCellSizeKey, 20f);
            _keyPanSpeed = EditorPrefs.GetFloat(keyPanSpeedKey, 20f);
            _zoomSpeed = EditorPrefs.GetFloat(zoomSpeedKey, 0.02f);
            _zoomMin = EditorPrefs.GetFloat(zoomMinKey, 0.2f);
            _zoomMax = EditorPrefs.GetFloat(zoomMaxKey, 1f);
            _autoSaveDelay = EditorPrefs.GetInt(autoSaveDelayKey, 5);
            _maxUndo = EditorPrefs.GetInt(maxUndoKey, 50);
            _showTooltips = EditorPrefs.GetBool(showTooltipsKey, true);
            _confirmDeletes = EditorPrefs.GetBool(confirmDeletesKey, true);
            _promptToSave = EditorPrefs.GetBool(promptToSaveKey, true);
            _pingAsset = EditorPrefs.GetBool(pingAssetKey, true);
            _visualDebug = EditorPrefs.GetBool(visualDebugKey, false);
            _showGrid = EditorPrefs.GetBool(showGridKey, true);
            _showTabTitle = EditorPrefs.GetBool(showTabTitleKey, false);
            _autoGenerateNameMap = EditorPrefs.GetBool(autoGenerateNameMapKey, true);
        }
    }
}
