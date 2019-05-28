/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Editor
{
    using UnityEditor;
    using UnityEngine;

    public class SettingsWindow : EditorWindow
    {
        private UserSettings _userSettings;
        private AIGeneralSettings _generalSettings;

        public static void ShowWindow()
        {
            EditorWindow.GetWindow<SettingsWindow>(true, "Apex AI Settings");
        }

        private void OnEnable()
        {
            _userSettings = UserSettings.instance;
            _generalSettings = AIGeneralSettings.instance;

            this.minSize = new Vector2(490f, 560f);
        }

        private void OnGUI()
        {
            EditorStyling.InitScaleAgnosticStyles();

            EditorGUIUtility.labelWidth = 200f;

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("User Settings");
            EditorGUILayout.BeginVertical(EditorStyling.Skinned.propertyBox);

            int selectedSkinIdx = Skins.options.IndexOf(_userSettings.skin);
            selectedSkinIdx = EditorGUILayout.Popup("Skin", selectedSkinIdx, Skins.options);
            if (Skins.options[selectedSkinIdx] != _userSettings.skin)
            {
                _userSettings.skin = Skins.options[selectedSkinIdx];
            }

            EditorGUILayout.Separator();
            var th = EditorGUILayout.FloatField(new GUIContent("Title UI Height", "The height of the title bar of Selector elements."), _userSettings.titleHeight, EditorStyling.Skinned.smallNumberField);
            if (th != _userSettings.titleHeight)
            {
                _userSettings.titleHeight = th;
            }

            var qh = EditorGUILayout.FloatField(new GUIContent("Qualifier UI Height", "The height of the Qualifier elements inside a Selector."), _userSettings.qualifierHeight, EditorStyling.Skinned.smallNumberField);
            if (qh != _userSettings.qualifierHeight)
            {
                _userSettings.qualifierHeight = qh;
            }

            var ah = EditorGUILayout.FloatField(new GUIContent("Action UI Height", "The height of the Action element inside a Qualifier."), _userSettings.actionHeight, EditorStyling.Skinned.smallNumberField);
            if (ah != _userSettings.actionHeight)
            {
                _userSettings.actionHeight = ah;
            }

            var sh = EditorGUILayout.FloatField(new GUIContent("Scorer UI Height", "The height of the Scorer elements inside a Qualifier during Visual Debugging."), _userSettings.scorerHeight, EditorStyling.Skinned.smallNumberField);
            if (sh != _userSettings.scorerHeight)
            {
                _userSettings.scorerHeight = sh;
            }

            EditorGUILayout.Separator();
            var cps = EditorGUILayout.FloatField(new GUIContent("Canvas Pan Sensitivity", "Controls how much the canvas pans when dragging the canvas with the mouse."), _userSettings.canvasPanSensitivity, EditorStyling.Skinned.smallNumberField);
            if (cps != _userSettings.canvasPanSensitivity)
            {
                _userSettings.canvasPanSensitivity = cps;
            }

            EditorGUILayout.Separator();
            var snap = EditorGUILayout.FloatField(new GUIContent("Snap Spacing", "The distance between snap points, i.e. the point where selectors will snap to when dragged around."), _userSettings.snapCellSize, EditorStyling.Skinned.smallNumberField);
            if (snap != _userSettings.snapCellSize)
            {
                _userSettings.snapCellSize = snap;
            }

            EditorGUILayout.Separator();
            var keyPanSpeed = EditorGUILayout.FloatField(new GUIContent("Key Pan Speed", "The speed with which key panning moves at, i.e. when using the arrow keys or numpad keys to pan the background."), _userSettings.keyPanSpeed, EditorStyling.Skinned.smallNumberField);
            if (keyPanSpeed != _userSettings.keyPanSpeed)
            {
                _userSettings.keyPanSpeed = keyPanSpeed;
            }

            EditorGUILayout.Separator();
            var zoomSpeed = EditorGUILayout.FloatField(new GUIContent("Zoom Speed", "The speed with which zooming is done."), _userSettings.zoomSpeed, EditorStyling.Skinned.smallNumberField);
            if (zoomSpeed != _userSettings.zoomSpeed)
            {
                _userSettings.zoomSpeed = zoomSpeed;
            }

            EditorGUILayout.BeginHorizontal();
            var zoomMin = EditorGUILayout.FloatField(new GUIContent("Zoom Scale", "The zoom scale range."), _userSettings.zoomMin, EditorStyling.Skinned.smallNumberField);
            if (zoomMin != _userSettings.zoomMin)
            {
                _userSettings.zoomMin = zoomMin;
            }

            var lblWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 20f;
            var zoomMax = EditorGUILayout.FloatField(new GUIContent("to"), _userSettings.zoomMax, EditorStyling.Skinned.smallNumberField);
            if (zoomMax != _userSettings.zoomMax)
            {
                _userSettings.zoomMax = zoomMax;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUIUtility.labelWidth = lblWidth;
            EditorGUILayout.Separator();
            var auto = EditorGUILayout.IntField(new GUIContent("Auto Save Interval", "The number of minutes between each automatic save. Set to 0 to disable auto-save."), _userSettings.autoSaveDelay, EditorStyling.Skinned.smallNumberField);
            if (auto != _userSettings.autoSaveDelay)
            {
                _userSettings.autoSaveDelay = auto;
            }

            EditorGUILayout.Separator();
            var mu = EditorGUILayout.IntField(new GUIContent("Max Undo", "The number undo operations before older operations begin to drop out."), _userSettings.maxUndo, EditorStyling.Skinned.smallNumberField);
            if (mu != _userSettings.maxUndo)
            {
                _userSettings.maxUndo = mu;
            }

            EditorGUILayout.Separator();
            var stt = EditorGUILayout.Toggle(new GUIContent("Show Tooltips", "Controls whether tooltips will be shown on editor elements."), _userSettings.showTooltips);
            if (stt != _userSettings.showTooltips)
            {
                _userSettings.showTooltips = stt;
            }

            var stit = EditorGUILayout.Toggle(new GUIContent("Show Title in Tab", "Controls whether the AI title is displayed (as much as can be) in the editor window title tab."), _userSettings.showTabTitle);
            if (stit != _userSettings.showTabTitle)
            {
                _userSettings.showTabTitle = stit;
            }

            var pa = EditorGUILayout.Toggle(new GUIContent("Ping Asset", "Controls whether the corresponding AI Asset is pinged when an AI Editor Window becomes focused."), _userSettings.pingAsset);
            if (pa != _userSettings.pingAsset)
            {
                _userSettings.pingAsset = pa;
            }

            EditorGUILayout.Separator();
            var cd = EditorGUILayout.Toggle(new GUIContent("Confirm Deletes", "Controls whether or not you will be prompted to confirm whenever you delete an item."), _userSettings.confirmDeletes);
            if (cd != _userSettings.confirmDeletes)
            {
                _userSettings.confirmDeletes = cd;
            }

            var pts = EditorGUILayout.Toggle(new GUIContent("Prompt to Save", "Controls whether or not you will be prompted to save unsaved changes. Untick this to auto-save whenever a prompt would normally happen."), _userSettings.promptToSave);
            if (pts != _userSettings.promptToSave)
            {
                _userSettings.promptToSave = pts;
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("General Settings");
            EditorGUILayout.BeginVertical(EditorStyling.Skinned.propertyBox);

            EditorGUILayout.LabelField("To change the storage location you must select a Resources folder in the project.", EditorStyling.Skinned.wrappedItalicText);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.TextField("Storage Location", _generalSettings.storagePath);

            if (GUILayout.Button("...", EditorStyling.Skinned.fixedButton))
            {
                var selectedFolder = EditorUtility.SaveFolderPanel("Select a Resources folder.", Application.dataPath, "Resources");
                if (!string.IsNullOrEmpty(selectedFolder))
                {
                    _generalSettings.storagePath = selectedFolder;
                    _generalSettings.SaveChanges();
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("The Name Map is a generated class used to reference AIs by name.", EditorStyling.Skinned.wrappedItalicText);
            var ag = EditorGUILayout.ToggleLeft(new GUIContent("Auto Generate", "If enabled this will automatically generate the AINameMap class when new AIs are created or AIs are deleted."), _userSettings.autoGenerateNameMap);
            if (ag != _userSettings.autoGenerateNameMap)
            {
                _userSettings.autoGenerateNameMap = ag;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.TextField("Name Map Location", _generalSettings.nameMapPath);

            if (GUILayout.Button("...", EditorStyling.Skinned.fixedButton))
            {
                var selectedFolder = EditorUtility.SaveFolderPanel("Select a location.", Application.dataPath, string.Empty);
                if (!string.IsNullOrEmpty(selectedFolder))
                {
                    _generalSettings.nameMapPath = selectedFolder;
                    _generalSettings.SaveChanges();
                }
            }

            if (GUILayout.Button("Generate Name Map", EditorStyling.Skinned.fixedButton))
            {
                AINameMapGenerator.WriteNameMapFile();

                EditorUtility.DisplayDialog("Generation Complete", "The AINameMap class was generated.\n\nIt can be accessed by including the Apex.AI name space.", "Ok");
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
    }
}
