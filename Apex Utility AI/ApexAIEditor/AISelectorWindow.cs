namespace Apex.AI.Editor
{
    using System;
    using Apex.AI.Serialization;
    using Apex.Editor;
    using UnityEngine;

    public sealed class AISelectorWindow : SelectorWindow<AIStorage>
    {
        private GUIContent _listItemContent = new GUIContent();
        private Action<AIStorage> _singleCallback;
        private Action<AIStorage[]> _multiCallback;

        public static AISelectorWindow Get(Vector2 screenPosition, Action<AIStorage> callback, bool allowNoneSelection = false)
        {
            var win = GetWindow<AISelectorWindow>(screenPosition, "Select an AI");
            win.Show(callback, allowNoneSelection);
            return win;
        }

        public static AISelectorWindow Get(Vector2 screenPosition, Action<AIStorage[]> callback, bool allowNoneSelection = false)
        {
            var win = GetWindow<AISelectorWindow>(screenPosition, "Select AIs");
            win.Show(callback, allowNoneSelection);
            return win;
        }

        private void Show(Action<AIStorage> callback, bool allowNoneSelection)
        {
            _singleCallback = callback;
            _multiCallback = null;
            Show(StoredAIs.AIs, RenderListItem, MatchItem, allowNoneSelection, false, OnSelect);
        }

        private void Show(Action<AIStorage[]> callback, bool allowNoneSelection)
        {
            _singleCallback = null;
            _multiCallback = callback;
            Show(StoredAIs.AIs, RenderListItem, MatchItem, allowNoneSelection, true, OnSelect);
        }

        private GUIContent RenderListItem(AIStorage ai)
        {
            _listItemContent.text = ai.name;
            _listItemContent.tooltip = ai.description;
            return _listItemContent;
        }

        private bool MatchItem(AIStorage ai, string search)
        {
            search = search.Replace(" ", string.Empty);
            var name = ai.name.Replace(" ", string.Empty);
            return name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void OnSelect(AIStorage[] items)
        {
            if (_multiCallback != null)
            {
                _multiCallback(items);
            }
            else if (items.Length > 0)
            {
                _singleCallback(items[0]);
            }
            else
            {
                _singleCallback(null);
            }

            this.Close();
        }
    }
}