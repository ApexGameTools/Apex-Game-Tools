/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using UnityEditor;
    using UnityEngine;

    internal static class ResourceManager
    {
        private static readonly Dictionary<string, Texture2D> _resources = new Dictionary<string, Texture2D>(StringComparer.Ordinal);
#if APEX_DLL
        private static readonly Dictionary<string, string> _resourceNames = new Dictionary<string, string>(StringComparer.Ordinal);
        private static readonly List<string> _loadedSkins = new List<string>();
#endif

        internal static Texture2D LoadPngResource(PngResource res)
        {
            Texture2D texture;
            if (_resources.TryGetValue(res.key, out texture) && texture)
            {
                return texture;
            }

            string subfolder;
            switch (res.skinning)
            {
                case SkinMode.Unity:
                {
                    subfolder = EditorGUIUtility.isProSkin ? "UnityPro" : "UnityFree";
                    break;
                }

                case SkinMode.Custom:
                {
                    subfolder = UserSettings.instance.skin;
                    break;
                }

                default:
                {
                    subfolder = "noSkin";
                    break;
                }
            }

#if APEX_DLL
            var asm = Assembly.GetCallingAssembly();

            string resourceName;
            if (!_resourceNames.TryGetValue(res.name, out resourceName))
            {
                if (_loadedSkins.Contains(subfolder))
                {
                    throw new ArgumentException(string.Format("No resource with the specified name '{0}', was found.", res.name));
                }

                var pngNamePicker = new Regex(string.Format(@".*\.{0}\.(.*)\.png", subfolder), RegexOptions.Compiled | RegexOptions.CultureInvariant);

                _loadedSkins.Add(subfolder);
                var resNames = asm.GetManifestResourceNames();
                for (int i = 0; i < resNames.Length; i++)
                {
                    var mg = pngNamePicker.Match(resNames[i]).Groups;
                    if (mg.Count != 2)
                    {
                        continue;
                    }

                    var key = mg[1].Value;
                    _resourceNames[key] = resNames[i];
                }

                if (!_resourceNames.TryGetValue(res.name, out resourceName))
                {
                    throw new ArgumentException(string.Format("No resource with the specified name '{0}', was found.", res.name));
                }
            }

            texture = new Texture2D(res.width, res.height, TextureFormat.ARGB32, false, true);
            texture.hideFlags = HideFlags.HideAndDontSave;
            using (var s = asm.GetManifestResourceStream(resourceName))
            {
                texture.LoadImage(ReadStream(s));
            }
#else
            texture = Resources.Load<Texture2D>(string.Concat(subfolder, "/", res.name));
            if (texture == null)
            {
                Debug.LogError(string.Concat("Could not load resource: ", res.name));
                return null;
            }
#endif
            _resources[res.key] = texture;
            return texture;
        }

        internal static void Reset()
        {
            _resources.Clear();
            _resourceNames.Clear();
            _loadedSkins.Clear();
        }

        private static byte[] ReadStream(Stream s)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = s.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }

                return ms.ToArray();
            }
        }
    }
}
