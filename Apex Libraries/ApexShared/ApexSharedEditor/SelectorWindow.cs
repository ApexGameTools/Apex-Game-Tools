/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Editor
{
    using System;
    using System.Collections.Generic;
    using Apex.Editor;
    using Apex.Utilities;
    using UnityEditor;
    using UnityEngine;

    public abstract class SelectorWindow<T> : EditorWindow
    {
        private IEnumerable<T> _itemList;
        private ListView<T> _listView;
        private Action<T[]> _onSelect;

        protected static TWin GetWindow<TWin>(Vector2 screenPosition, string title) where TWin : EditorWindow
        {
            var host = EditorWindow.focusedWindow;
            var win = EditorWindow.GetWindow<TWin>(true, title, false);
            win.position = PopupConstraints.GetValidPosition(win.position, screenPosition, host);

            return win;
        }

        public void Preselect(Func<T, bool> predicate, string filter = null)
        {
            _listView.Select(predicate, filter);
            Repaint();
        }

        protected void Show(IEnumerable<T> items, Func<T, GUIContent> itemRenderer, Func<T, string, bool> searchPredicate, bool allowNoneSelection, bool allowMultiSelect, Action<T[]> onSelect = null)
        {
            _itemList = items;
            allowNoneSelection = allowNoneSelection && (onSelect != null);
            _listView = new ListView<T>(this, itemRenderer, searchPredicate, allowNoneSelection, allowMultiSelect, onSelect);
            
            _onSelect = onSelect;

            this.Show();
            this.Focus();
        }

        protected void RenderInfo()
        {
            var msg = _listView.multiSelectEnabled ? "Select one or more items in the list and press Enter to confirm.\nYou can also double-click a single item to select it." : "Double-click an item to select it.";
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField(msg, SharedStyles.BuiltIn.wrappedText);
            EditorGUILayout.EndVertical();
        }

        private void OnLostFocus()
        {
            //Closing directly here causes an exception for some reason, however delaying it a frame solves it
            SafeClose();
        }

        private void SafeClose()
        {
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnEditorUpdate()
        {
            EditorApplication.update -= OnEditorUpdate;
            this.Close();
        }

        private void OnGUI()
        {
            if (_itemList == null)
            {
                // if an important reference is lost, close now
                SafeClose();
                return;
            }

            RenderInfo();
            _listView.Render(_itemList);
        }
    }
}