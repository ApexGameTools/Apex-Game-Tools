/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Editor
{
    using System;
    using System.IO;
    using System.Text.RegularExpressions;
    using UnityEditor;
    using UnityEngine;

    public static class AssetPath
    {
        private static readonly Regex _assetsFolderMatch = new Regex("(?:^|/)(Assets)(?:/|$)", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);
        private static readonly Regex _assetFolderSubPath = new Regex("Assets(.*?)/?$", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

        public static string GetApexRoot(bool relative)
        {
            string relativePath = "Assets/Apex";
            string fullPath = GetFullPath(relativePath);
            if (Directory.Exists(fullPath))
            {
                return relative ? relativePath : fullPath;
            }
#if APEX_DLL
            fullPath = Parent(NormalizePath(Path.GetDirectoryName(typeof(ApexComponentAttribute).Assembly.Location)));
#else
            var keyFile = Directory.GetFiles(Application.dataPath, "ApexComponentAttribute.cs", SearchOption.AllDirectories);
            if (keyFile.Length == 1)
            {
                fullPath = Parent(NormalizePath(Path.GetDirectoryName(keyFile[0])));
            }
            else
            {
                fullPath = Combine(Application.dataPath, "Apex");
            }
#endif
            return relative ? ProjectRelativePath(fullPath) : fullPath;
        }

        public static string GetApexDataFolder(bool relative)
        {
            return Combine(GetApexRoot(relative), "Editor/Data");
        }

        public static string Parent(string path)
        {
            var idx = path.LastIndexOf('/');
            if (idx < 0)
            {
                return string.Empty;
            }

            return path.Substring(0, idx);
        }

        public static string Combine(string partOne, string partTwo)
        {
            return string.Concat(
                NormalizePath(partOne),
                "/",
                NormalizePath(partTwo));
        }

        public static string NormalizePath(string path)
        {
            return path.Replace('\\', '/').TrimEnd('/');
        }

        public static bool IsProjectPath(string path)
        {
            return AssetsFolderIndex(NormalizePath(path)) >= 0;
        }

        public static string ProjectRelativePath(string path)
        {
            path = NormalizePath(path);
            int relativeIdx = AssetsFolderIndex(path);
            if (relativeIdx < 0)
            {
                throw new ArgumentException("The path is not a project path.");
            }

            return path.Substring(relativeIdx);
        }

        public static string GetFullPath(string projectRelativePath)
        {
            var idx = AssetsFolderIndex(projectRelativePath);
            if (idx != 0)
            {
                throw new ArgumentException("The path is not a project relative path.");
            }

            return string.Concat(Application.dataPath, _assetFolderSubPath.Match(projectRelativePath).Groups[1].Value);
        }

        public static void EnsurePath(string path)
        {
            path = NormalizePath(path);

            //Due to Unity being buggy, using AssetPathToGUID to test if a folder exists does not work. If the folder is deleted it will still be reported as existing.
            //Using Directory.Exists as well solves part of this. However adding, deleting and once again adding a named folder will fail, as AssetPathToGUID will say the folder exists but trying to add assets to it will fail stating the folder does not exist, thanks Unity....
            var fullPath = GetFullPath(path);
            if (!Directory.Exists(fullPath) || string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(path)))
            {
                var subhierarchy = path.Split('/');
                var parent = subhierarchy[0];
                for (int i = 1; i < subhierarchy.Length; i++)
                {
                    var subFolder = subhierarchy[i];
                    var subPath = Combine(parent, subFolder);
                    fullPath = GetFullPath(subPath);
                    if (!Directory.Exists(fullPath) || string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(subPath)))
                    {
                        AssetDatabase.CreateFolder(parent, subFolder);
                    }

                    parent = subPath;
                }
            }
        }

        private static int AssetsFolderIndex(string normalizedPath)
        {
            var m = _assetsFolderMatch.Match(normalizedPath);
            if (!m.Success)
            {
                return -1;
            }

            return m.Groups[1].Index;
        }
    }
}
