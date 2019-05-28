/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Editor
{
    using System;
    using System.IO;
    using System.Linq;
    using Apex.Editor;
    using UnityEditor;
    using UnityEngine;

    public class AIGeneralSettings : ScriptableObject
    {
        internal const string NameMapFileName = "AINameMap.cs";
        private static AIGeneralSettings _instance;

        [SerializeField, HideInInspector]
        private string _storagePath;

        [SerializeField, HideInInspector]
        private string _nameMapPath;

        private bool _isDirty;

        internal static AIGeneralSettings instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                var dataFolder = AssetPath.GetApexDataFolder(true);
                var loadPath = AssetPath.Combine(dataFolder, "AIGeneralSettings.asset");

                AssetPath.EnsurePath(dataFolder);

                _instance = AssetDatabase.LoadAssetAtPath(loadPath, typeof(AIGeneralSettings)) as AIGeneralSettings;

                if (_instance != null)
                {
                    return _instance;
                }

                //No settings found so create some
                _instance = ScriptableObject.CreateInstance<AIGeneralSettings>();

                //We need to find a suitable place to store ai configurations. They must be stored in a resources folder, so find the first one.
                string assetsFolder = Application.dataPath;

                string outmostResourceFolder = AssetPath.Combine(assetsFolder, "Resources");

                _instance._storagePath = AssetPath.Combine(
                    AssetPath.ProjectRelativePath(outmostResourceFolder),
                    AIManager.StorageFolder);

                AssetPath.EnsurePath(_instance._storagePath);

                AssetDatabase.CreateAsset(_instance, loadPath);
                AssetDatabase.SaveAssets();

                return _instance;
            }
        }

        internal string storagePath
        {
            get
            {
                return _storagePath;
            }

            set
            {
                if (ChangeStorageFolder(value))
                {
                    _isDirty = true;
                }
            }
        }

        internal string nameMapPath
        {
            get
            {
                return _nameMapPath;
            }

            set
            {
                if (ChangeNameMapFolder(value))
                {
                    _isDirty = true;
                }
            }
        }

        internal bool isDirty
        {
            get { return _isDirty; }
        }

        internal void SaveChanges()
        {
            if (_isDirty)
            {
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
                _isDirty = false;
            }
        }

        private bool ChangeStorageFolder(string proposedFolder)
        {
            if (string.Equals(proposedFolder, _storagePath, StringComparison.OrdinalIgnoreCase) || !AssetPath.IsProjectPath(proposedFolder))
            {
                return false;
            }

            proposedFolder = AssetPath.ProjectRelativePath(proposedFolder);
            bool apexified = proposedFolder.EndsWith("Resources/" + AIManager.StorageFolder);

            if (!proposedFolder.EndsWith("Resources") && !apexified)
            {
                EditorUtility.DisplayDialog("Invalid Storage Folder", "The storage folder selected must be a Resources folder. This can however be anywhere inside the Assets folder.", "Ok");
                return false;
            }

            if (!apexified)
            {
                proposedFolder = AssetPath.Combine(proposedFolder, AIManager.StorageFolder);
            }

            AssetPath.EnsurePath(proposedFolder);

            //Move files from current storage location to new location.
            var fullStoragePath = AssetPath.GetFullPath(_storagePath);
            if (Directory.Exists(fullStoragePath))
            {
                foreach (var asset in Directory.GetFiles(fullStoragePath, "*.asset", SearchOption.TopDirectoryOnly))
                {
                    var fileName = Path.GetFileName(asset);
                    var msg = AssetDatabase.MoveAsset(AssetPath.ProjectRelativePath(asset), AssetPath.Combine(proposedFolder, fileName));
                    if (!string.IsNullOrEmpty(msg))
                    {
                        EditorUtility.DisplayDialog("Error Moving Assets", msg, "Ok");
                        return false;
                    }
                }

                AssetDatabase.DeleteAsset(_storagePath);
            }

            _storagePath = proposedFolder;

            return true;
        }

        private bool ChangeNameMapFolder(string proposedFolder)
        {
            if (string.Equals(proposedFolder, _nameMapPath, StringComparison.OrdinalIgnoreCase) || !AssetPath.IsProjectPath(proposedFolder))
            {
                return false;
            }

            proposedFolder = AssetPath.ProjectRelativePath(proposedFolder);
            AssetPath.EnsurePath(proposedFolder);

            //Move map from current location to new location.
            if (!string.IsNullOrEmpty(_nameMapPath))
            {
                var existingFile = AssetPath.Combine(AssetPath.GetFullPath(_nameMapPath), NameMapFileName);

                if (File.Exists(existingFile))
                {
                    var oldFilePath = AssetPath.Combine(_nameMapPath, NameMapFileName);
                    var newFilePath = AssetPath.Combine(proposedFolder, NameMapFileName);

                    var msg = AssetDatabase.MoveAsset(oldFilePath, newFilePath);
                    if (!string.IsNullOrEmpty(msg))
                    {
                        EditorUtility.DisplayDialog("Error Moving Asset", msg, "Ok");
                        return false;
                    }
                    else
                    {
                        AssetDatabase.Refresh();
                    }
                }
            }

            _nameMapPath = proposedFolder;

            return true;
        }
    }
}
