namespace Apex.AI.Editor
{
    using System;
    using System.Linq;
    using Apex.AI.Editor.Reflection;
    using Apex.Editor;
    using UnityEngine;

    public sealed class AIEntitySelectorWindow : SelectorWindow<AIBuildingBlocks.NamedType>
    {
        private GUIContent _listItemContent = new GUIContent();
        private Action<Type> _singleCallback;
        private Action<Type[]> _multiCallback;

        public static void Get<T>(Vector2 screenPosition, Action<Type> callback)
        {
            Get(screenPosition, typeof(T), callback);
        }

        public static void Get(Vector2 screenPosition, Type baseType, Action<Type> callback)
        {
            var title = string.Concat(DisplayHelper.GetFriendlyName(baseType).TrimStart('I').Trim(), " Search | AI Object Selector");
            var win = GetWindow<AIEntitySelectorWindow>(screenPosition, title);
            win.Show(baseType, callback);
        }

        public static void Get<T>(Vector2 screenPosition, Action<Type[]> callback)
        {
            Get(screenPosition, typeof(T), callback);
        }

        public static void Get(Vector2 screenPosition, Type baseType, Action<Type[]> callback)
        {
            var title = string.Concat(DisplayHelper.GetFriendlyName(baseType).TrimStart('I').Trim(), " Search | AI Object Selector");
            var win = GetWindow<AIEntitySelectorWindow>(screenPosition, title);
            win.Show(baseType, callback);
        }

        private void Show(Type baseType, Action<Type> callback)
        {
            _singleCallback = callback;
            _multiCallback = null;
            Show(AIBuildingBlocks.Instance.GetForType(baseType), RenderListItem, MatchItem, false, false, OnSelect);
        }

        private void Show(Type baseType, Action<Type[]> callback)
        {
            _singleCallback = null;
            _multiCallback = callback;
            Show(AIBuildingBlocks.Instance.GetForType(baseType), RenderListItem, MatchItem, false, true, OnSelect);
        }

        private GUIContent RenderListItem(AIBuildingBlocks.NamedType entity)
        {
            _listItemContent.text = entity.friendlyName;
            _listItemContent.tooltip = entity.description;
            return _listItemContent;
        }

        private bool MatchItem(AIBuildingBlocks.NamedType entity, string search)
        {
            search = search.Replace(" ", string.Empty);
            var name = entity.friendlyName.Replace(" ", string.Empty);
            return name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void OnSelect(AIBuildingBlocks.NamedType[] items)
        {
            if (_multiCallback != null)
            {
                _multiCallback(items.Select(item => item.type).ToArray());
            }
            else if (items.Length > 0)
            {
                _singleCallback(items[0].type);
            }

            if (AIEditorWindow.activeInstance != null)
            {
                AIEditorWindow.activeInstance.Repaint();
            }

            this.Close();
        }
    }
}