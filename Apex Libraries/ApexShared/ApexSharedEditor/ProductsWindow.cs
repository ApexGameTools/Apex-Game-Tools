/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Apex.Editor.Versioning;
    using UnityEditor;
    using UnityEditorInternal;
    using UnityEngine;

    //[InitializeOnLoad]
    public class ProductsWindow : EditorWindow
    {
        private static IEnumerable<ProductInfo> _productsPreloaded;
        private IEnumerable<ProductInfo> _products;
        private Styles _styles;
        private Vector2 _scrollPos;
        private ApexSettings _settings;

        //static ProductsWindow()
        //{
        //    if (!EditorApplication.isPlayingOrWillChangePlaymode)
        //    {
        //        EditorApplication.delayCall += CheckAutoShow;
        //    }
        //}

        //private static void CheckAutoShow()
        //{
        //    EditorApplication.delayCall -= CheckAutoShow;
        //    if (EditorApplication.isPlaying)
        //    {
        //        return;
        //    }

        //    ApexSettings settings;
        //    if (!ApexSettings.TryGetSettings(out settings))
        //    {
        //        if (settings == null)
        //        {
        //            EditorUtility.DisplayDialog("Apex Error", "Apex Product was unable to create its settings file, please report this to the Apex Team.", "Ok");
        //            return;
        //        }

        //        settings.allowAutomaticUpdateCheck = EditorUtility.DisplayDialog("Apex Update", "Apex Products can check for availability of new versions.\n\nThese checks will not install anything new, nor will they send any information whatsoever.\n\nDo you wish to enable automatic version checks?", "Yes", "No");
        //        settings.SaveChanges();
        //    }

        //    if (settings.timeToUpdate)
        //    {
        //        ProductManager.GetProductsInfoAsync(settings, true, ShowIfUdates);
        //    }
        //}

        //private static void ShowIfUdates(IEnumerable<ProductInfo> products)
        //{
        //    if (products.Any(p => p.newUpdateAvailable))
        //    {
        //        EditorUtility.DisplayDialog("Apex Update", "An Apex product update is available.", "Ok");

        //        _productsPreloaded = products;
        //        SharedMenuExtentions.ProductsWindow();
        //    }
        //}

        private void OnEnable()
        {
            this.minSize = new Vector2(490f, 350f);

            if (!ApexSettings.TryGetSettings(out _settings) && _settings == null)
            {
                return;
            }

            //Get the products list
            if (_productsPreloaded != null)
            {
                _products = _productsPreloaded;
                _productsPreloaded = null;
            }
            else
            {
                ProductManager.GetProductsInfoAsync(
                    _settings,
                    false,
                    prods =>
                    {
                        _products = prods;
                    });
            }

            _styles = new Styles();
        }

        private void OnGUI()
        {
            if (_products == null)
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("Loading...", _styles.centeredText);
                GUILayout.FlexibleSpace();
                this.Repaint();
                return;
            }

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, false, false, GUIStyle.none, GUI.skin.verticalScrollbar, GUIStyle.none);

            CreateProductEntries(ProductType.Product);
            CreateProductEntries(ProductType.Extension);
            CreateProductEntries(ProductType.Example);

            EditorGUILayout.EndScrollView();

            //CreateFooter();

            if (_settings.isDirty)
            {
                _settings.SaveChanges();
            }
        }

        private void CreateProductEntries(ProductType type)
        {
            var entries = _products.Where(p => p.type == type);
            if (entries.Any())
            {
                EditorGUILayout.Separator();
                EditorGUILayout.LabelField(type.ToString() + "s");
                foreach (var p in entries)
                {
                    CreateProductEntry(p);
                }
            }
        }

        private void CreateProductEntry(ProductInfo p)
        {
            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.BeginHorizontal();

            //Create and anchor the foldout
            GUILayout.Label(GUIContent.none);

            var productLabel = new GUIContent(" " + p.name, p.icon.texture);
            p.expanded = EditorGUI.Foldout(GUILayoutUtility.GetLastRect(), p.expanded, productLabel, true, _styles.foldout);

            //New update marker
            if (p.newUpdateAvailable)
            {
                if (p.status == ProductStatus.PatchAvailable)
                {
                    GUILayout.Label(new GUIContent("#", "New patch available!"), _styles.labelHighlight, GUILayout.Width(20));
                }
                else
                {
                    GUILayout.Label(new GUIContent("!", "New update available!"), _styles.labelHighlight, GUILayout.Width(20));
                }
            }
            else
            {
                GUILayout.Label(GUIContent.none, GUILayout.Width(20));
            }

            //Version
            GUIContent status;
            if (p.installedVersion == null)
            {
                status = new GUIContent("N/A", "Not installed");
            }
            else
            {
                status = new GUIContent(Write(p.installedVersion), "Installed version");
            }

            GUILayout.Label(status, _styles.centeredText, GUILayout.Width(50));

            //Link buttons / status
            if (p.status != ProductStatus.UpToDate)
            {
                if (p.status == ProductStatus.ComingSoon)
                {
                    EditorGUILayout.LabelField("Coming soon", GUILayout.Width(80));
                }
                else if (p.status == ProductStatus.UpdateAvailable)
                {
                    if (GUILayout.Button(new GUIContent("Update It", string.Format("Version {0} is now available.\n\nClick to open the product's page on the Unity Asset Store.", p.newestVersion)), EditorStyles.toolbarButton, GUILayout.Width(80)))
                    {
                        AssetStore.Open(p.storeUrl);
                    }
                }
                else if (p.status == ProductStatus.ProductAvailable)
                {
                    if (GUILayout.Button(new GUIContent("Get It", "Now available.\n\nClick to open the product's page on the Unity Asset Store."), EditorStyles.toolbarButton, GUILayout.Width(80)))
                    {
                        AssetStore.Open(p.storeUrl);
                    }
                }
                else if (p.status == ProductStatus.PatchAvailable)
                {
                    if (GUILayout.Button(new GUIContent("Patch It", "A patch is available.\n\nClick to download and apply the patch. Patches are hot fixes and all patches will be included in the following product update."), EditorStyles.toolbarButton, GUILayout.Width(80)))
                    {
                        ProductManager.ApplyPatches(p, _settings);
                    }
                }
            }
            else
            {
                GUILayout.Space(80f);
            }

            EditorGUILayout.EndHorizontal();

            //Details
            if (p.expanded)
            {
                GUILayout.Box(GUIContent.none, _styles.boxSeparator);

                var description = string.IsNullOrEmpty(p.description) ? "No description available" : p.description;
                EditorGUILayout.LabelField(description, _styles.labelWithWrap);

                EditorGUILayout.BeginHorizontal();

                if (!string.IsNullOrEmpty(p.productUrl))
                {
                    if (GUILayout.Button("Product Page", _styles.labelLink))
                    {
                        Application.OpenURL(p.productUrl);
                    }

                    EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
                }

                if (!string.IsNullOrEmpty(p.changeLogPath))
                {
                    if (GUILayout.Button("Change Log", _styles.labelLink))
                    {
                        Application.OpenURL(p.changeLogPath);
                    }

                    EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        private string Write(Version v)
        {
            int fields = 2;

            //Due to the Version class having been implemented by a complete moron,
            //we have to take special care to not request a higher field count that is available
            if (v.Revision > 0)
            {
                fields += 2;
            }
            else if (v.Build > 0)
            {
                fields++;
            }

            return v.ToString(fields);
        }

        private void CreateFooter()
        {
            EditorGUILayout.BeginVertical("Box");

            //Auto update settings
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Automatic update checking is ", _styles.labelNoStretch);

            if (_settings.allowAutomaticUpdateCheck)
            {
                if (GUILayout.Button("on", _styles.labelOn))
                {
                    _settings.allowAutomaticUpdateCheck = !_settings.allowAutomaticUpdateCheck;
                }

                EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);

                GUILayout.Label(" every ", _styles.labelNoStretch);
                _settings.updateCheckIntervalHours = EditorGUILayout.IntField(_settings.updateCheckIntervalHours, _styles.smallIntField, GUILayout.Width(40f));
                GUILayout.Label(" hours.", _styles.labelNoStretch);
            }
            else
            {
                if (GUILayout.Button("off", _styles.labelOff))
                {
                    _settings.allowAutomaticUpdateCheck = !_settings.allowAutomaticUpdateCheck;
                }

                EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
            }

            EditorGUILayout.EndHorizontal();

            //Last update check info
            EditorGUILayout.Separator();
            EditorGUILayout.BeginHorizontal();
            string lastUpdateCheck = GetTimeSinceUpdate();
            GUILayout.Label("Last update check was", _styles.labelNoStretch);
            GUILayout.Label(lastUpdateCheck, _styles.labelNoStretch);
            EditorGUILayout.EndHorizontal();

            //Manual update
            if (GUILayout.Button("Check for Updates Now"))
            {
                _products = null;
                ProductManager.GetProductsInfoAsync(
                    _settings,
                    true,
                    prods =>
                    {
                        _products = prods;
                    });
            }

            EditorGUILayout.EndVertical();
        }

        private string GetTimeSinceUpdate()
        {
            if (!_settings.lastUpdateCheck.HasValue)
            {
                return "never";
            }

            var diff = DateTime.UtcNow - _settings.lastUpdateCheck.Value;
            if (diff.Days > 0)
            {
                return string.Concat(diff.Days, " days ago");
            }
            else if (diff.Hours > 0)
            {
                return string.Concat(diff.Hours, " hours ago");
            }
            else if (diff.Minutes > 0)
            {
                return string.Concat(diff.Minutes, " minutes ago");
            }

            return "a moment ago";
        }

        private class Styles
        {
            private Dictionary<string, GUIStyle> _styles = new Dictionary<string, GUIStyle>();

            internal GUIStyle foldout
            {
                get
                {
                    GUIStyle style;
                    if (!_styles.TryGetValue("foldout", out style))
                    {
                        style = new GUIStyle(EditorStyles.foldout);
                        style.padding = new RectOffset(20, 0, 0, 0);
                        style.imagePosition = ImagePosition.ImageLeft;
                        style.fixedWidth = 250f;

                        _styles.Add("foldout", style);
                    }

                    return style;
                }
            }

            internal GUIStyle centeredText
            {
                get
                {
                    GUIStyle style;
                    if (!_styles.TryGetValue("centeredText", out style))
                    {
                        style = new GUIStyle(EditorStyles.label);
                        style.alignment = TextAnchor.MiddleCenter;

                        _styles.Add("centeredText", style);
                    }

                    return style;
                }
            }

            internal GUIStyle labelWithWrap
            {
                get
                {
                    GUIStyle style;
                    if (!_styles.TryGetValue("labelWithWrap", out style))
                    {
                        style = new GUIStyle(EditorStyles.label);
                        style.wordWrap = true;

                        _styles.Add("labelWithWrap", style);
                    }

                    return style;
                }
            }

            internal GUIStyle labelWarn
            {
                get
                {
                    GUIStyle style;
                    if (!_styles.TryGetValue("labelWarn", out style))
                    {
                        style = new GUIStyle(labelWithWrap);
                        style.normal.textColor = new Color(179 / 255f, 175 / 255f, 112 / 255f);

                        _styles.Add("labelWarn", style);
                    }

                    return style;
                }
            }

            internal GUIStyle labelLink
            {
                get
                {
                    GUIStyle style;
                    if (!_styles.TryGetValue("labelLink", out style))
                    {
                        style = new GUIStyle(EditorStyles.label);
                        style.normal.textColor = new Color(163 / 255f, 73 / 255f, 164 / 255f);
                        style.stretchWidth = false;

                        _styles.Add("labelLink", style);
                    }

                    return style;
                }
            }

            internal GUIStyle labelNoStretch
            {
                get
                {
                    GUIStyle style;
                    if (!_styles.TryGetValue("labelNoStretch", out style))
                    {
                        style = new GUIStyle(EditorStyles.label);
                        style.stretchWidth = false;

                        _styles.Add("labelNoStretch", style);
                    }

                    return style;
                }
            }

            internal GUIStyle labelHighlight
            {
                get
                {
                    GUIStyle style;
                    if (!_styles.TryGetValue("labelHighlight", out style))
                    {
                        style = new GUIStyle(EditorStyles.label);
                        style.normal.textColor = Color.green;
                        style.fontStyle = FontStyle.Bold;
                        style.stretchWidth = false;

                        _styles.Add("labelHighlight", style);
                    }

                    return style;
                }
            }

            internal GUIStyle labelOn
            {
                get
                {
                    GUIStyle style;
                    if (!_styles.TryGetValue("labelOn", out style))
                    {
                        style = new GUIStyle(EditorStyles.label);
                        style.normal.textColor = Color.green;
                        style.stretchWidth = false;

                        _styles.Add("labelOn", style);
                    }

                    return style;
                }
            }

            internal GUIStyle labelOff
            {
                get
                {
                    GUIStyle style;
                    if (!_styles.TryGetValue("labelOff", out style))
                    {
                        style = new GUIStyle(EditorStyles.label);
                        style.normal.textColor = Color.red;
                        style.stretchWidth = false;

                        _styles.Add("labelOff", style);
                    }

                    return style;
                }
            }

            internal GUIStyle smallIntField
            {
                get
                {
                    GUIStyle style;
                    if (!_styles.TryGetValue("smallIntField", out style))
                    {
                        style = new GUIStyle(GUI.skin.textField);
                        style.stretchWidth = false;
                        style.alignment = TextAnchor.MiddleRight;

                        _styles.Add("smallIntField", style);
                    }

                    return style;
                }
            }

            internal GUIStyle boxSeparator
            {
                get
                {
                    GUIStyle style;
                    if (!_styles.TryGetValue("boxSeparator", out style))
                    {
                        style = new GUIStyle(GUI.skin.box);
                        style.stretchWidth = true;
                        style.fixedHeight = 3f;

                        _styles.Add("boxSeparator", style);
                    }

                    return style;
                }
            }
        }
    }
}
