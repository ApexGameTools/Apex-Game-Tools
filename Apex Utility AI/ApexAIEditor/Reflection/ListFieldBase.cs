/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Editor.Reflection
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using Apex.AI.Editor.UndoRedo;
    using Apex.Editor;
    using Apex.Serialization;
    using UnityEditor;
    using UnityEngine;
    using Visualization;

    public abstract class ListFieldBase : IEditorField
    {
        private const float _dragRectHeight = 40f;
        private const float _dragHandleHeight = 16f;
        private const float _dragHandleWidth = 16f;
        private const float _dragHandlePaddingX = 5f;
        private const float _dragHandlePaddingY = 2f;

        protected object _owner;
        protected IList _list;
        protected Type _itemType;
        protected Action<IList> _setter;
        private List<EditorItem> _editorItems;
        private GUIContent _label;
        private bool _isSimpleType;
        private SimpleItemConstructor _simpleItemCreator;

        private bool _reorderable;
        private bool _dragging;
        private Rect _draggedRect;
        private float _dragOffsetY;
        private int _draggingIndex = -1;
        private int _dropIndex = -1;

        protected ListFieldBase(MemberData data, object owner, bool requiresSetter)
        {
            _owner = owner;
            _label = new GUIContent(data.name, data.description);
            this.memberName = data.member.Name;

            var prop = data.member as PropertyInfo;
            if (prop != null)
            {
                _reorderable = !prop.IsDefined<NotReorderableAttribute>(true);
                _itemType = GetItemType(prop.PropertyType);
                _list = (IList)prop.GetValue(owner, null);
                if (_list == null || requiresSetter)
                {
                    _setter = (list) =>
                    {
                        _list = list;
                        prop.SetValue(owner, list, null);
                        if (_simpleItemCreator != null)
                        {
                            _simpleItemCreator.SetParent(_list);
                        }
                    };
                }
            }
            else
            {
                var field = data.member as FieldInfo;
                if (field != null)
                {
                    _reorderable = !field.IsDefined<NotReorderableAttribute>(true);
                    _itemType = GetItemType(field.FieldType);
                    _list = (IList)field.GetValue(owner);
                    if (_list == null || requiresSetter)
                    {
                        _setter = (list) =>
                        {
                            _list = list;
                            field.SetValue(owner, list);
                            if (_simpleItemCreator != null)
                            {
                                _simpleItemCreator.SetParent(_list);
                            }
                        };
                    }
                }
                else
                {
                    throw new ArgumentException("Invalid reflected member type, only fields and properties are supported.");
                }
            }

            _isSimpleType = SerializationMaster.ConverterExists(_itemType) || SerializationMaster.StagerExists(_itemType);

            if (_list != null)
            {
                var count = _list.Count;
                _editorItems = new List<EditorItem>(count);

                if (_isSimpleType)
                {
                    _simpleItemCreator = new SimpleItemConstructor(_list, _itemType);

                    for (int i = 0; i < count; i++)
                    {
                        var item = _simpleItemCreator.Conctruct(i, _list[i]);
                        _editorItems.Add(ReflectMaster.Reflect(item));
                    }
                }
                else
                {
                    for (int i = 0; i < count; i++)
                    {
                        _editorItems.Add(ReflectMaster.Reflect(_list[i]));
                    }
                }
            }
            else
            {
                _editorItems = new List<EditorItem>();
            }
        }

        public string memberName
        {
            get;
            private set;
        }

        public object currentValue
        {
            get { return _list; }
        }

        protected abstract Type GetItemType(Type listType);

        protected abstract void DoRemove(int removeIdx, AIInspectorState state);

        protected abstract void DoAdd(object item, AIInspectorState state);

        protected abstract IList CreateList();

        public void RenderField(AIInspectorState state)
        {
            var evt = Event.current;
            var mousePos = evt.mousePosition;
            var boxStyle = new GUIStyle("Box");

            if (_reorderable && _dragging)
            {
                // don't let the mouse affect the X position of the rect being dragged
                mousePos.x = _draggedRect.x;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(_label);
            EditorGUILayout.Separator();
            if (GUILayout.Button(SharedStyles.addTooltip, SharedStyles.BuiltIn.addButtonSmall))
            {
                AddNew(mousePos, state);
            }

            EditorGUILayout.EndHorizontal();

            var count = _editorItems.Count;
            if (count == 0)
            {
                return;
            }

            EditorGUILayout.BeginVertical(boxStyle);

            var removeIdx = -1;
            if (!_reorderable || count <= 1)
            {
                for (int i = 0; i < count; i++)
                {
                    // if list is not reorderable, just draw items
                    EditorGUILayout.BeginVertical();
                    if (DrawEditorItem(_editorItems[i], state, false))
                    {
                        removeIdx = i;
                    }

                    EditorGUILayout.EndVertical();
                }
            }
            else
            {
                // Reorderable list
                int drawIdx = 0;
                for (int i = 0; i < count; i++)
                {
                    if (_dragging && _draggingIndex == drawIdx)
                    {
                        drawIdx++;
                    }

                    Rect dragHandle;
                    Rect itemRect;
                    if (i == _dropIndex)
                    {
                        dragHandle = new Rect();
                        itemRect = EditorGUILayout.BeginVertical();
                        GUILayout.Space(_dragRectHeight);
                        EditorGUILayout.EndVertical();
                    }
                    else
                    {
                        itemRect = EditorGUILayout.BeginVertical();
                        var item = _editorItems[drawIdx++];
                        if (DrawEditorItem(item, state, true))
                        {
                            removeIdx = i;
                        }

                        EditorGUILayout.EndVertical();

                        dragHandle = DrawDragHandle();
                    }

                    if (evt.button == MouseButton.left && evt.isMouse)
                    {
                        if (!_dragging)
                        {
                            if (dragHandle.Contains(mousePos) && evt.type == EventType.MouseDrag)
                            {
                                _dragging = true;
                                _draggingIndex = _dropIndex = i;
                                _draggedRect = itemRect;
                                _dragOffsetY = itemRect.y - mousePos.y;
                                break;
                            }
                        }
                        else
                        {
                            if (itemRect.Contains(mousePos))
                            {
                                _dropIndex = i;
                                break;
                            }
                        }
                    }
                }

                if (_dragging && evt.type == EventType.MouseUp)
                {
                    if (_draggingIndex != _dropIndex)
                    {
                        _list.Reorder(_draggingIndex, _dropIndex);
                        _editorItems.Reorder(_draggingIndex, _dropIndex);
                        state.currentAIUI.undoRedo.Do(new ReorderOperation(_draggingIndex, _dropIndex, _list));
                        state.MarkDirty();

                        if (Application.isPlaying)
                        {
                            HandleVisualizerReorder(state, _draggingIndex, _dropIndex);
                        }
                    }

                    _dragging = false;
                    _dropIndex = _draggingIndex = -1;
                }
            }

            EditorGUILayout.EndVertical();

            if (removeIdx >= 0 && DisplayHelper.ConfirmDelete("item"))
            {
                DoRemove(removeIdx, state);
                _editorItems.RemoveAt(removeIdx);
                state.MarkDirty();

                if (Application.isPlaying)
                {
                    HandleVisualizerRemove(state, removeIdx);
                }
            }

            if (_reorderable && _dragging)
            {
                if (evt.type == EventType.Layout || evt.type == EventType.Repaint)
                {
                    GUI.Box(new Rect(_draggedRect.x, mousePos.y + _dragOffsetY, _draggedRect.width, _dragRectHeight), _editorItems[_draggingIndex].name, boxStyle);
                    AIInspectorEditor.instance.Repaint();
                }
            }
        }

        private bool DrawEditorItem(EditorItem item, AIInspectorState state, bool spaceForHandle)
        {
            var result = false;

            // Item Header
            EditorGUILayout.BeginHorizontal(spaceForHandle ? SharedStyles.BuiltIn.listItemHeaderIndented : SharedStyles.BuiltIn.listItemHeader);

            var cbd = item.item as ICanBeDisabled;
            if (cbd != null)
            {
                var isDisabled = !EditorGUILayout.ToggleLeft(item.name, !cbd.isDisabled, cbd.isDisabled ? EditorStyling.Skinned.disabledText : SharedStyles.BuiltIn.normalText);
                if (isDisabled != cbd.isDisabled)
                {
                    cbd.isDisabled = isDisabled;
                    state.currentAIUI.undoRedo.Do(new DisableOperation(cbd));
                }
            }
            else
            {
                EditorGUILayout.LabelField(item.name, SharedStyles.BuiltIn.normalText);
            }

            if (GUILayout.Button(SharedStyles.deleteTooltip, SharedStyles.BuiltIn.deleteButtonSmall))
            {
                result = true;

                // We do not want the button click itself to count as a change. Since no other changes are expected when a button click is encountered, we can just set it to false.
                GUI.changed = false;
            }

            EditorGUILayout.EndHorizontal();

            // Item fields
            item.Render(state);
            EditorGUILayout.Separator();

            return result;
        }

        private Rect DrawDragHandle()
        {
            var rect = GUILayoutUtility.GetLastRect();
            var handleRect = new Rect(rect.x + _dragHandlePaddingX, rect.y + _dragHandlePaddingY, _dragHandleWidth, _dragHandleHeight);
            GUI.Label(handleRect, new GUIContent(UIResources.InspectorDragHandle.texture), EditorStyling.Canvas.smallButtonIcon);
            EditorGUIUtility.AddCursorRect(handleRect, MouseCursor.MoveArrow);

            return handleRect;
        }

        private void AddNew(Vector2 mousePos, AIInspectorState state)
        {
            if (_isSimpleType)
            {
                if (_list == null)
                {
                    _setter(CreateList());
                }

                if (_simpleItemCreator == null)
                {
                    _simpleItemCreator = new SimpleItemConstructor(_list, _itemType);
                }

                var item = Activator.CreateInstance(_itemType);
                DoAdd(item, state);

                var itemWrapper = _simpleItemCreator.Conctruct(_list.Count - 1, item);
                _editorItems.Add(ReflectMaster.Reflect(itemWrapper));
                return;
            }

            Action<Type[]> cb = (selectedTypes) =>
            {
                if (_list == null)
                {
                    _setter(CreateList());
                }

                using (state.currentAIUI.undoRedo.bulkOperation)
                {
                    for (int i = 0; i < selectedTypes.Length; i++)
                    {
                        var selectedType = selectedTypes[i];
                        if (_itemType.IsGenericType && selectedType.IsGenericType)
                        {
                            var genArgs = _itemType.GetGenericArguments();
                            selectedType = selectedType.GetGenericTypeDefinition().MakeGenericType(genArgs);
                        }

                        var item = Activator.CreateInstance(selectedType);
                        DoAdd(item, state);
                        _editorItems.Add(ReflectMaster.Reflect(item));

                        if (Application.isPlaying)
                        {
                            HandleVisualizerAdd(state, item);
                        }
                    }

                    state.MarkDirty();
                }
            };

            // We do not want the button click itself to count as a change.. same as above.
            GUI.changed = false;

            var screenPos = EditorGUIUtility.GUIToScreenPoint(mousePos);
            AIEntitySelectorWindow.Get(screenPos, _itemType, cb);
        }

        //Functionality specific to Composite Visualizers
        private IEnumerable<ICompositeVisualizer> GetVisualizers(AIInspectorState state)
        {
            if (!VisualizationManager.isVisualizing)
            {
                yield break;
            }

            var isCompositeQualifier = (_owner is ICompositeScorer) && (_owner is IQualifier);
            var isCompositeAction = _owner is CompositeAction;
            if (!isCompositeQualifier && !isCompositeAction)
            {
                yield break;
            }

            var ui = state.currentAIUI;
            if (ui == null)
            {
                yield break;
            }

            var clientsToUpdate = AIManager.GetAIClients(ui.ai.id);

            int count = clientsToUpdate.Count;
            for (int i = 0; i < count; i++)
            {
                var ai = clientsToUpdate[i].ai as UtilityAIVisualizer;
                if (ai != null)
                {
                    if (isCompositeQualifier)
                    {
                        yield return ai.FindQualifierVisualizer((IQualifier)_owner) as ICompositeVisualizer;
                    }
                    else
                    {
                        yield return ai.FindActionVisualizer((IAction)_owner) as ICompositeVisualizer;
                    }
                }
            }
        }

        private void HandleVisualizerReorder(AIInspectorState state, int fromIdx, int toIdx)
        {
            var visualizers = GetVisualizers(state);
            foreach (var cv in visualizers)
            {
                cv.children.Reorder(fromIdx, toIdx);
            }
        }

        protected void HandleVisualizerAdd(AIInspectorState state, object item)
        {
            var visualizers = GetVisualizers(state);
            foreach (var cv in visualizers)
            {
                cv.Add(item);
            }
        }

        private void HandleVisualizerRemove(AIInspectorState state, int idx)
        {
            var visualizers = GetVisualizers(state);
            foreach (var cv in visualizers)
            {
                cv.children.RemoveAt(idx);
            }
        }

        private class SimpleItemConstructor
        {
            private static readonly Type _baseType = typeof(SimpleListItem<>);

            private Type _itemType;
            private object[] _ctorParams;

            internal SimpleItemConstructor(IList parent, Type itemType)
            {
                _itemType = _baseType.MakeGenericType(itemType);
                _ctorParams = new object[3];
                _ctorParams[2] = parent;
            }

            internal void SetParent(IList parent)
            {
                _ctorParams[2] = parent;
            }

            internal object Conctruct(int idx, object value)
            {
                _ctorParams[0] = value;
                _ctorParams[1] = idx;
                return Activator.CreateInstance(_itemType, _ctorParams);
            }
        }
    }
}