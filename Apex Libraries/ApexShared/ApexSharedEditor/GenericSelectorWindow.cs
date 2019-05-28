/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public sealed class GenericSelectorWindow : SelectorWindow<object>
    {
        public static void Show<T>(Vector2 screenPosition, string title, IEnumerable<T> items, Func<T, GUIContent> itemRenderer, Func<T, string, bool> searchPredicate, bool allowNoneSelection, bool allowMultiSelect, Action<T[]> onSelect = null) where T : class
        {
            var win = GetWindow<GenericSelectorWindow>(screenPosition, title);

            var handler = new Handler<T>
            {
                _parent = win,
                _items = items,
                _itemRenderer = itemRenderer,
                _searchPredicate = searchPredicate,
                _onSelectCallback = onSelect
            };

            win.Show(
                handler.items,
                handler.RenderListItem,
                handler.SearchList,
                allowNoneSelection,
                allowMultiSelect,
                handler.OnSelect);
        }

        private class Handler<T> where T : class
        {
            internal GenericSelectorWindow _parent;
            internal IEnumerable<T> _items;
            internal Action<T[]> _onSelectCallback;
            internal Func<T, GUIContent> _itemRenderer;
            internal Func<T, string, bool> _searchPredicate;

            internal IEnumerable<object> items
            {
                get { return _items.Cast<object>(); }
            }

            internal GUIContent RenderListItem(object o)
            {
                return _itemRenderer((T)o);
            }

            internal bool SearchList(object o, string search)
            {
                return _searchPredicate((T)o, search);
            }

            internal void OnSelect(object[] items)
            {
                if (_onSelectCallback != null)
                {
                    _onSelectCallback(items.Cast<T>().ToArray());
                }

                _parent.Close();
            }
        }
    }
}