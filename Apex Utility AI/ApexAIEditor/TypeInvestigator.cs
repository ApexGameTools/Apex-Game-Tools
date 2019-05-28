/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Apex.Editor;
    using Apex.Serialization;
    using Serialization;
    using UnityEditor;
    using UnityEngine;
    using Utilities;

    internal class TypeInvestigator
    {
        private static readonly string[] _aiTabTitles = new string[] { "Selected AIs", "All AIs" };
        private static readonly string[] _groupingTabTitles = new string[] { "Referenced Type", "Unreferenced Type", "AI" };

        private EditorWindow _owner;
        private List<TypeNameTokens> _unreferencedTypes;
        private List<ReferencedType> _referencedTypes;
        private ReferencingAI[] _referencingAIs;
        private GUIContent _listItemContent = new GUIContent();
        private Vector2 _typesScrollPos;
        private Vector2 _aiScrollPos;
        private ListView<AIStorage> _aiSelectView;
        private Step _step;
        private TypeGrouping _grouping;
        private AISelection _aiSelection;
        private bool _excludeApexTypes = true;

        public TypeInvestigator(EditorWindow owner)
        {
            _owner = owner;
            _unreferencedTypes = new List<TypeNameTokens>();
            _referencedTypes = new List<ReferencedType>();
        }

        private enum Step
        {
            Start,
            Processing,
            Result
        }

        private enum TypeGrouping
        {
            ReferencedType,
            UnreferencedType,
            AI
        }

        private enum AISelection
        {
            Selected,
            All
        }

        private ListView<AIStorage> aiListView
        {
            get
            {
                if (_aiSelectView == null)
                {
                    _aiSelectView = new ListView<AIStorage>(_owner, RenderListItem, MatchItem, false, true, (ais) => CreateOverview(ais));
                }

                return _aiSelectView;
            }
        }

        internal void Reset()
        {
            _aiSelection = AISelection.Selected;
            _aiScrollPos = _typesScrollPos = Vector2.zero;
            _aiSelectView = null;
            _step = Step.Start;
        }

        internal void Render()
        {
            switch (_step)
            {
                case Step.Start:
                {
                    DrawStart();
                    break;
                }

                case Step.Processing:
                {
                    DrawWorking();
                    break;
                }

                case Step.Result:
                {
                    DrawResult();
                    break;
                }
            }
        }

        private GUIContent RenderListItem(AIStorage ai)
        {
            _listItemContent.text = ai.name;
            return _listItemContent;
        }

        private bool MatchItem(AIStorage ai, string search)
        {
            search = search.Replace(" ", string.Empty);
            var name = ai.name.Replace(" ", string.Empty);
            return name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void DrawStart()
        {
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("This tool allows you to get an overview of which serializable types are in use in which AIs.\n\nThe tool can identify types currently serialized with at least one AI.", SharedStyles.BuiltIn.wrappedText);
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Select which AI(s) to investigate.", SharedStyles.BuiltIn.centeredWrappedText);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            _aiSelection = (AISelection)GUILayout.SelectionGrid((int)_aiSelection, _aiTabTitles, 2, EditorStyles.toolbarButton);
            GUILayout.FlexibleSpace();
            EditorGUIUtility.labelWidth = 120f;
            _excludeApexTypes = EditorGUILayout.Toggle("Exclude Apex types", _excludeApexTypes);
            EditorGUIUtility.labelWidth = 0f;
            EditorGUILayout.EndHorizontal();

            if (_aiSelection == AISelection.Selected)
            {
                this.aiListView.Render(StoredAIs.AIs);
            }
            else
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("Fixed Selection.", SharedStyles.BuiltIn.centeredWrappedText);
                GUILayout.FlexibleSpace();
            }

            if (GUILayout.Button("Create Overview"))
            {
                var ais = (_aiSelection == AISelection.Selected) ? this.aiListView.GetSelectedItems() : StoredAIs.AIs.ToArray();
                CreateOverview(ais);
            }
        }

        private void DrawWorking()
        {
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Working on it....", SharedStyles.BuiltIn.centeredWrappedText);
            GUILayout.FlexibleSpace();
        }

        private void DrawResult()
        {
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Below you can see which types are referenced by which AIs, and which are not referenced at all.", SharedStyles.BuiltIn.wrappedText);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button("Expand All", EditorStyles.toolbarButton))
            {
                _referencedTypes.Apply(s => s.expanded = true);
                _referencingAIs.Apply(ai => ai.expanded = true);
            }

            if (GUILayout.Button("Collapse All", EditorStyles.toolbarButton))
            {
                _referencedTypes.Apply(s => s.expanded = false);
                _referencingAIs.Apply(ai => ai.expanded = false);
            }

            if (GUILayout.Button("Copy", EditorStyles.toolbarButton))
            {
                EditorGUIUtility.systemCopyBuffer = Export();
            }

            GUILayout.FlexibleSpace();

            EditorGUILayout.LabelField("Group by", GUILayout.Width(60f));
            _grouping = (TypeGrouping)GUILayout.SelectionGrid((int)_grouping, _groupingTabTitles, 3, EditorStyles.toolbarButton);
            EditorGUILayout.EndHorizontal();

            if (_grouping == TypeGrouping.ReferencedType)
            {
                _typesScrollPos = EditorGUILayout.BeginScrollView(_typesScrollPos, "Box");
                EditorGUILayout.LabelField("Referenced Types", SharedStyles.BuiltIn.centeredWrappedText);
                foreach (var t in _referencedTypes)
                {
                    DrawTypeListItem(t);
                }

                EditorGUILayout.EndScrollView();
            }
            else if (_grouping == TypeGrouping.UnreferencedType)
            {
                _typesScrollPos = EditorGUILayout.BeginScrollView(_typesScrollPos, "Box");
                EditorGUILayout.LabelField("Unreferenced Types", SharedStyles.BuiltIn.centeredWrappedText);
                foreach (var t in _unreferencedTypes)
                {
                    DrawTypeListItem(t);
                }

                EditorGUILayout.EndScrollView();
            }
            else
            {
                _aiScrollPos = EditorGUILayout.BeginScrollView(_aiScrollPos, "Box");
                EditorGUILayout.LabelField("AIs", SharedStyles.BuiltIn.centeredWrappedText);
                foreach (var ai in _referencingAIs)
                {
                    DrawAIListItem(ai);
                }

                EditorGUILayout.EndScrollView();
            }

            if (GUILayout.Button("Back"))
            {
                _aiScrollPos = _typesScrollPos = Vector2.zero;
                _step = Step.Start;
            }
        }

        private void DrawTypeListItem(ReferencedType t)
        {
            var labelContent = new GUIContent();

            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.BeginHorizontal(SharedStyles.BuiltIn.listItemHeader);
            labelContent.text = t.fullName;
            EditorGUILayout.LabelField(labelContent);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical("Box");
            labelContent.text = "Referenced by";
            EditorGUILayout.LabelField(labelContent, SharedStyles.BuiltIn.centeredWrappedText);
            t.expanded = EditorGUI.Foldout(GUILayoutUtility.GetLastRect(), t.expanded, GUIContent.none, true);
            if (t.expanded)
            {
                foreach (var aiRef in t.referencingAIs)
                {
                    labelContent.text = aiRef.name;
                    EditorGUILayout.LabelField(labelContent);
                }
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
        }

        private void DrawTypeListItem(TypeNameTokens t)
        {
            var labelContent = new GUIContent();

            EditorGUILayout.BeginVertical("Box");
            labelContent.text = t.fullTypeName;
            EditorGUILayout.LabelField(labelContent);
            EditorGUILayout.EndVertical();
        }

        private void DrawAIListItem(ReferencingAI ai)
        {
            var labelContent = new GUIContent();

            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.BeginHorizontal(SharedStyles.BuiltIn.listItemHeader);
            labelContent.text = ai.name;
            EditorGUILayout.LabelField(labelContent);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical("Box");
            labelContent.text = "References";
            EditorGUILayout.LabelField(labelContent, SharedStyles.BuiltIn.centeredWrappedText);
            ai.expanded = EditorGUI.Foldout(GUILayoutUtility.GetLastRect(), ai.expanded, GUIContent.none, true);
            if (ai.expanded)
            {
                foreach (var t in ai.referencedTypes)
                {
                    labelContent.text = t.fullName;
                    EditorGUILayout.LabelField(labelContent);
                }
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
        }

        private void CreateOverview(AIStorage[] ais)
        {
            _step = Step.Processing;
            EditorAsync.ExecuteDelayed(() => Investigate(ais), 10);

            _owner.Repaint();
        }

        private void Investigate(AIStorage[] ais)
        { 
            var availableTypes = from t in ApexReflection.GetRelevantTypes()
                                 where !t.IsAbstract && (!_excludeApexTypes || t.Namespace == null || !t.Namespace.StartsWith("Apex.AI")) && (t.IsDefined<ApexSerializedTypeAttribute>(true) || SerializationMaster.GetSerializedProperties(t).Any() || SerializationMaster.GetSerializedFields(t).Any())
                                 orderby t.FullName
                                 select new TypeNameTokens(t.AssemblyQualifiedName);

            var aiLookup = new Dictionary<string, ReferencingAI>(StringComparer.Ordinal);
            foreach (var ai in ais)
            {
                aiLookup.Add(ai.aiId, new ReferencingAI(ai.name));
            }

            _referencedTypes.Clear();
            _unreferencedTypes.Clear();
            foreach (var t in availableTypes)
            {
                var refType = new ReferencedType(t.fullTypeName);

                foreach (var ai in ais)
                {
                    if (ai.configuration.Contains(t.completeTypeName))
                    {
                        var refAI = aiLookup[ai.aiId];
                        refAI.referencedTypes.Add(refType);
                        refType.referencingAIs.Add(refAI);
                    }
                }

                if (refType.referencingAIs.Count > 0)
                {
                    _referencedTypes.Add(refType);
                }
                else
                {
                    _unreferencedTypes.Add(t);
                }
            }

            _referencingAIs = (from ai in aiLookup.Values
                               where ai.referencedTypes.Count > 0
                               orderby ai.name
                               select ai).ToArray();

            _step = Step.Result;
            _owner.Repaint();
        }

        private string Export()
        {
            StringBuilder b = new StringBuilder();
            if (_grouping == TypeGrouping.ReferencedType)
            {
                foreach (var t in _referencedTypes)
                {
                    b.AppendFormat("[{0}]", t.fullName);
                    b.AppendLine();
                    foreach (var aiRef in t.referencingAIs)
                    {
                        b.AppendLine(aiRef.name);
                    }

                    b.AppendLine();
                }
            }
            else if (_grouping == TypeGrouping.UnreferencedType)
            {
                foreach (var t in _unreferencedTypes)
                {
                    b.AppendLine(t.fullTypeName);
                }
            }
            else
            {
                foreach (var ai in _referencingAIs)
                {
                    b.AppendFormat("[{0}]", ai.name);
                    b.AppendLine();
                    foreach (var t in ai.referencedTypes)
                    {
                        b.AppendLine(t.fullName);
                    }

                    b.AppendLine();
                }
            }

            return b.ToString();
        }

        private class ReferencedType
        {
            internal string fullName;
            internal List<ReferencingAI> referencingAIs;
            internal bool expanded = true;

            public ReferencedType(string fullName)
            {
                this.fullName = fullName;
                referencingAIs = new List<ReferencingAI>();
            }
        }

        private class ReferencingAI
        {
            internal string name;
            internal List<ReferencedType> referencedTypes;
            internal bool expanded = true;

            public ReferencingAI(string name)
            {
                this.name = name;
                referencedTypes = new List<ReferencedType>();
            }
        }
    }
}
