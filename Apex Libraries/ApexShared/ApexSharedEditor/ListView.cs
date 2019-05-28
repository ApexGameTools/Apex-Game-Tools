/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Apex.Utilities;
    using UnityEditor;
    using UnityEngine;

    public sealed class ListView<T>
    {
        private EditorWindow _owner;
        private Func<T, GUIContent> _itemRenderer;
        private Func<T, string, bool> _searchPredicate;
        private Func<T, bool> _selectPredicate;
        private Action<T[]> _onSelect;

        private IEnumerable<T> _itemList;
        private T[] _filteredList;

        private bool _allowMultiSelect;
        private bool _allowNoneSelect;
        private int _selectRangeStart = -1;
        private int _selectRangeEnd = -1;
        private HashSet<int> _selectedIndexes;
        private bool _singleSelected;
        private bool _noneSelected;

        private string _searchString = string.Empty;
        private Vector2 _scrollPos = Vector2.zero;

        public ListView(EditorWindow owner, Func<T, GUIContent> itemRenderer, Func<T, string, bool> searchPredicate, bool allowNoneSelect, bool allowMultiSelect, Action<T[]> onSelect = null)
        {
            _owner = owner;
            _itemRenderer = itemRenderer;
            _searchPredicate = searchPredicate;
            _allowMultiSelect = allowMultiSelect;
            _allowNoneSelect = allowNoneSelect;
            _onSelect = onSelect;
            _selectedIndexes = new HashSet<int>();
        }

        public bool hasSelected
        {
            get { return _selectedIndexes.Count > 0; }
        }

        public bool multiSelectEnabled
        {
            get { return _allowMultiSelect; }
        }

        public bool noneSelectEnabled
        {
            get { return _allowNoneSelect; }
        }

        public T[] GetSelectedItems()
        {
            //Since this control sets the keyboard Control it causes it to carry over to subsequent views if not reset.
            GUIUtility.keyboardControl = 0;

            if (_noneSelected)
            {
                return Empty<T>.array;
            }

            var count = _selectedIndexes.Count;
            if (count == 0)
            {
                return Empty<T>.array;
            }

            var arr = new T[count];
            int i = 0;
            foreach (var index in _selectedIndexes.OrderBy(idx => idx))
            {
                arr[i++] = _filteredList[index];
            }

            return arr;
        }

        public void Reset()
        {
            _itemList = null;
            _filteredList = null;

            _selectRangeStart = _selectRangeEnd = -1;
            _selectedIndexes.Clear();
            _singleSelected = false;
            _noneSelected = false;

            _searchString = string.Empty;
            _scrollPos = Vector2.zero;
        }

        //Calling this before render will select the items matching the predicate and optionally set a filter (first)
        //This only needs to be called once and not for every render, i.e. no need to call this in OnGUI.
        public void Select(Func<T, bool> predicate, string filter = null)
        {
            _selectPredicate = predicate;
            if (filter != null)
            {
                _filteredList = null;
                _searchString = filter;
            }
        }

        public void Render(IEnumerable<T> itemList, float elementHeight = 20f, float elementPadding = 1f)
        {
            GUILayout.FlexibleSpace();
            var rect = GUILayoutUtility.GetLastRect();
            rect.width = _owner.position.width;

            Render(rect, itemList, elementHeight, elementPadding);
        }

        public void Render(Rect rect, IEnumerable<T> itemList, float elementHeight = 20f, float elementPadding = 1f)
        {
            var totalElementHeight = elementHeight + elementPadding;
            var width = rect.width;
            var height = rect.height;

            if (!object.ReferenceEquals(itemList, _itemList))
            {
                _itemList = itemList;
                _filteredList = null;
            }

            GUI.BeginGroup(rect);

            // Search Field
            var searchFieldRect = new Rect(5f, 5f, width - 25f, elementHeight);
            var lastSearchString = _searchString;
            GUI.SetNextControlName("Apex_ListView_Search");
            _searchString = EditorGUI.TextField(searchFieldRect, _searchString, SharedStyles.BuiltIn.searchFieldStyle);
            EditorGUI.FocusTextInControl("Apex_ListView_Search");

            var searchCancelRect = new Rect(searchFieldRect.x + searchFieldRect.width, 5f, 10f, elementHeight);
            if (GUI.Button(searchCancelRect, GUIContent.none, string.IsNullOrEmpty(_searchString) ? SharedStyles.BuiltIn.searchCancelButtonEmpty : SharedStyles.BuiltIn.searchCancelButton))
            {
                _searchString = string.Empty;
                GUIUtility.keyboardControl = 0;
            }

            // filter list based on search string
            if (_filteredList == null || !string.Equals(lastSearchString, _searchString, StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrEmpty(_searchString))
                {
                    _filteredList = _itemList as T[];
                    if (_filteredList == null)
                    {
                        _filteredList = _itemList.ToArray();
                    }
                }
                else
                {
                    _filteredList = _itemList.Where(t => _searchPredicate(t, _searchString)).ToArray();
                }

                _selectRangeStart = _selectRangeEnd = -1;
                _selectedIndexes.Clear();
            }

            // Scroll View
            var noneItemAdjust = _allowNoneSelect ? 1 : 0;
            float listStartY = searchFieldRect.y + searchFieldRect.height + 5f;
            var scrollViewRect = new Rect(0f, listStartY, width, height - listStartY);
            var innerScrollView = new Rect(0f, 0f, scrollViewRect.width - 20f, (_filteredList.Length + noneItemAdjust) * totalElementHeight);

            //Do preselect if applicable
            var evt = Event.current;
            if (evt != null && evt.type == EventType.Repaint && _selectPredicate != null)
            {
                _selectedIndexes.AddRange(_filteredList.Select((item, idx) => _selectPredicate(item) ? idx : -1).Where(idx => idx >= 0));
                if (_selectedIndexes.Count > 0)
                {
                    _selectRangeStart = _selectRangeEnd = _selectedIndexes.Min();
                    EnsureInView(_selectRangeStart, totalElementHeight, scrollViewRect.height);
                }

                _selectPredicate = null;
            }

            _scrollPos = GUI.BeginScrollView(scrollViewRect, _scrollPos, innerScrollView);

            // Element List
            int startIdx = Math.Max(Mathf.FloorToInt(_scrollPos.y / totalElementHeight) - noneItemAdjust, 0);
            int endIdx = Math.Min(startIdx + Mathf.CeilToInt(Mathf.Min(scrollViewRect.height, innerScrollView.height) / totalElementHeight), _filteredList.Length);

            if (_allowNoneSelect && _scrollPos.y < totalElementHeight)
            {
                var itemRect = new Rect(2f, 0f, innerScrollView.width, elementHeight);

                var style = _noneSelected ? SharedStyles.BuiltIn.objectSelectorBoxActive : SharedStyles.BuiltIn.objectSelectorBoxNormal;
                GUI.Box(itemRect, "None", style);
            }

            var currentY = (startIdx + noneItemAdjust) * totalElementHeight;
            for (var i = startIdx; i < endIdx; i++)
            {
                var item = _filteredList[i];
                var itemRect = new Rect(2f, currentY, innerScrollView.width, elementHeight);

                var style = _selectedIndexes.Contains(i) ? SharedStyles.BuiltIn.objectSelectorBoxActive : SharedStyles.BuiltIn.objectSelectorBoxNormal;
                GUI.Box(itemRect, _itemRenderer(item), style);

                currentY += totalElementHeight;
            }

            GUI.EndScrollView();

            //Handle mouse clicks and key presses
            if (evt.type == EventType.MouseDown && evt.button == MouseButton.left)
            {
                evt.Use();
                float adjustedMouseY = evt.mousePosition.y + (_scrollPos.y - listStartY);
                var index = Mathf.FloorToInt(adjustedMouseY / totalElementHeight) - noneItemAdjust;

                if (_allowNoneSelect && index < 0)
                {
                    SelectNone();
                }
                else if (_allowMultiSelect)
                {
                    if (evt.command || evt.control)
                    {
                        SingleToggle(index);
                    }
                    else if (evt.shift)
                    {
                        RangeSelect(index);
                    }
                    else
                    {
                        SingleSelect(index);
                    }
                }
                else
                {
                    SingleSelect(index);
                }

                //We allow double-click selection under all circumstances
                _singleSelected = (_onSelect != null) && (evt.clickCount == 2) && (_selectedIndexes.Count == 1 || _noneSelected);
                if (!_singleSelected)
                {
                    _owner.Repaint();
                }
            }
            else if (evt.type == EventType.MouseUp && evt.button == MouseButton.left)
            {
                evt.Use();

                if (_singleSelected)
                {
                    // double click detected
                    DoSelect();
                }
            }
            else if (evt.type == EventType.KeyUp)
            {
                if (evt.keyCode == KeyCode.DownArrow)
                {
                    evt.Use();

                    if (_selectRangeStart < 0 && _allowNoneSelect && !_noneSelected)
                    {
                        SelectNone();
                    }
                    else if (_allowMultiSelect && evt.shift)
                    {
                        RangeSelect(_selectRangeEnd + 1);
                    }
                    else
                    {
                        SingleSelect(_selectRangeStart + 1);
                    }

                    EnsureInView(_selectRangeEnd, totalElementHeight, scrollViewRect.height);
                }
                else if (evt.keyCode == KeyCode.UpArrow)
                {
                    evt.Use();

                    if (_selectRangeStart == 0 && _selectRangeEnd == 0 && _allowNoneSelect)
                    {
                        SelectNone();
                    }
                    else if (_selectRangeEnd < 0 && !_noneSelected)
                    {
                        SingleSelect(_filteredList.Length - 1);
                    }
                    else if (_allowMultiSelect && evt.shift)
                    {
                        RangeSelect(_selectRangeEnd - 1);
                    }
                    else
                    {
                        SingleSelect(_selectRangeStart - 1);
                    }

                    EnsureInView(_selectRangeStart, totalElementHeight, scrollViewRect.height);
                }
                else if (evt.keyCode == KeyCode.A && (evt.control || evt.command))
                {
                    evt.Use();
                    _noneSelected = false;
                    AddSelected(0, _filteredList.Length - 1);
                }
                else if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                {
                    evt.Use();

                    if (_onSelect != null && (_selectedIndexes.Count > 0 || _noneSelected))
                    {
                        DoSelect();
                    }
                }
            }

            GUI.EndGroup();
        }

        private void EnsureInView(int itemIdx, float totalElementHeight, float viewPortHeight)
        {
            var adjust = _allowNoneSelect ? 1 : 0;
            var maxPos = (itemIdx + adjust) * totalElementHeight;
            var minPos = Mathf.Max((maxPos - viewPortHeight) + totalElementHeight, 0f);
            if (_scrollPos.y > maxPos)
            {
                _scrollPos.y = maxPos;
            }
            else if (_scrollPos.y < minPos)
            {
                _scrollPos.y = minPos;
            }
        }

        private void DoSelect()
        {
            _onSelect(GetSelectedItems());
        }

        private void SelectNone()
        {
            _selectedIndexes.Clear();
            _selectRangeStart = _selectRangeEnd = -1;
            _noneSelected = true;
        }

        private void SingleToggle(int index)
        {
            if (index < 0 || index >= _filteredList.Length)
            {
                return;
            }

            _noneSelected = false;
            if (_selectedIndexes.Add(index))
            {
                _selectRangeStart = _selectRangeEnd = index;
            }
            else
            {
                _selectedIndexes.Remove(index);
                _selectRangeStart = _selectRangeEnd = -1;
            }
        }

        private void SingleSelect(int index)
        {
            if (index < 0 || index >= _filteredList.Length)
            {
                return;
            }

            _noneSelected = false;
            _selectRangeStart = _selectRangeEnd = index;
            _selectedIndexes.Clear();
            _selectedIndexes.Add(index);
        }

        private void RangeSelect(int index)
        {
            if (index < 0 || index >= _filteredList.Length)
            {
                return;
            }

            _noneSelected = false;
            if (_selectRangeStart < 0)
            {
                //In this case we just let it act similar to if control was down except for the deselect part
                _selectedIndexes.Add(index);
                _selectRangeStart = _selectRangeEnd = index;
            }
            else
            {
                if (_selectRangeEnd >= _selectRangeStart)
                {
                    if (index < _selectRangeEnd)
                    {
                        RemoveSelected(Math.Max(index, _selectRangeStart) + 1, _selectRangeEnd);
                        AddSelected(index, _selectRangeStart - 1);
                    }
                    else
                    {
                        AddSelected(_selectRangeEnd + 1, index);
                    }
                }
                else
                {
                    if (index > _selectRangeEnd)
                    {
                        RemoveSelected(_selectRangeEnd, Math.Min(index, _selectRangeStart) - 1);
                        AddSelected(_selectRangeStart + 1, index);
                    }
                    else
                    {
                        AddSelected(index, _selectRangeEnd - 1);
                    }
                }

                _selectRangeEnd = index;
            }
        }

        private void RemoveSelected(int from, int to)
        {
            for (int i = from; i <= to; i++)
            {
                _selectedIndexes.Remove(i);
            }
        }

        private void AddSelected(int from, int to)
        {
            for (int i = from; i <= to; i++)
            {
                _selectedIndexes.Add(i);
            }
        }
    }
}
