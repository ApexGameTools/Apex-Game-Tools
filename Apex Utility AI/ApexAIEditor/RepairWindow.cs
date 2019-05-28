/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Editor
{
    using System;
    using System.Linq;
    using System.Text;
    using Apex.AI.Serialization;
    using Apex.Editor;
    using UnityEditor;
    using UnityEngine;

    public class RepairWindow : EditorWindow
    {
        private Step _step = 0;
        private bool _working;
        private Vector2 _scrollPos;
        private RepairUtility.UnresolvedType[] _typeResolution;
        private RepairUtility.MemberResolutionInfo[] _memberResolution;
        private RepairUtility.MismatchedMemberInfo[] _mismatchResolution;
        private RepairUtility.ResolutionStatus _status;

        private RepairUtility _utility;
        private ListView<AIStorage> _listView;
        private GUIContent _listItemContent;

        private enum Step
        {
            Start,
            Types,
            Members,
            Mismatch,
            Summary
        }

        public static void ShowWindow()
        {
            var win = EditorWindow.GetWindow<RepairWindow>(true, string.Empty);
            win.SetTitle("Apex AI Repair", UIResources.EditorWindowIcon.texture);
        }

        public static void ShowWindow(string filter, params string[] selectedAIIds)
        {
            var win = EditorWindow.GetWindow<RepairWindow>(true, string.Empty);
            win.SetTitle("Apex AI Repair", UIResources.EditorWindowIcon.texture);
            win._listView.Select(ai => selectedAIIds.Contains(ai.aiId), filter);
        }

        private void OnEnable()
        {
            _utility = new RepairUtility();
            _listItemContent = new GUIContent();
            _listView = new ListView<AIStorage>(this, RenderListItem, MatchItem, false, true, ais => StartResolutionAsync(ais));
            this.minSize = new Vector2(520f, 495f);
        }

        private void OnDisable()
        {
            _step = Step.Start;
        }

        private GUIContent RenderListItem(AIStorage ai)
        {
            _listItemContent.text = ai.name;
            return _listItemContent;
        }

        private GUIContent RenderListItem(TypeNameTokens t)
        {
            _listItemContent.text = t.fullTypeName;
            return _listItemContent;
        }

        private GUIContent RenderListItem(string s)
        {
            _listItemContent.text = s;
            return _listItemContent;
        }

        private bool MatchItem(AIStorage ai, string search)
        {
            search = search.Replace(" ", string.Empty);
            var name = ai.name.Replace(" ", string.Empty);
            return name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool MatchItem(TypeNameTokens t, string search)
        {
            search = search.Replace(" ", string.Empty);
            var name = t.simpleTypeName.Replace(" ", string.Empty);
            return name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool MatchItem(string s, string search)
        {
            return s.StartsWith(search, StringComparison.OrdinalIgnoreCase);
        }

        private void OnGUI()
        {
            EditorStyling.InitScaleAgnosticStyles();

            if (_working)
            {
                DrawWorking();
                return;
            }

            switch (_step)
            {
                case Step.Start:
                {
                    DrawStart();
                    break;
                }

                case Step.Types:
                {
                    DrawTypeFix();
                    break;
                }

                case Step.Members:
                {
                    DrawPropFix();
                    break;
                }

                case Step.Mismatch:
                {
                    DrawMismatchFix();
                    break;
                }

                case Step.Summary:
                {
                    DrawSummary();
                    break;
                }
            }
        }

        private void StartResolutionAsync(AIStorage[] ais)
        {
            _working = true;

            EditorAsync.Execute(
                    () =>
                    {
                        _utility.Begin(ais);
                        _typeResolution = _utility.IdentifyUnresolvedTypes();

                        if (_typeResolution.Length > 0)
                        {
                            _step = Step.Types;
                        }
                        else
                        {
                            _memberResolution = _utility.IdentifyUnresolvedMembers();
                            if (_memberResolution.Length > 0)
                            {
                                _step = Step.Members;
                            }
                            else
                            {
                                _mismatchResolution = _utility.IdentifyMismatchedMembers();
                                if (_mismatchResolution.Length > 0)
                                {
                                    _step = Step.Mismatch;
                                }
                                else
                                {
                                    _status = _utility.GetStatus();
                                    _step = Step.Summary;
                                }
                            }
                        }
                    },
                    () => EndWork());
        }

        private void EndWork()
        {
            _working = false;
            this.Repaint();
        }

        private void ToStart()
        {
            if (EditorUtility.DisplayDialog("Confirm", "Going back to change the AI selection will reset any resolutions already made.\n\nDo you wish to go back?", "Yes", "No"))
            {
                _scrollPos = Vector2.zero;
                _step = Step.Start;
            }
        }

        private void DrawWorking()
        {
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Working on it....", SharedStyles.BuiltIn.centeredWrappedText);
            GUILayout.FlexibleSpace();
        }

        private void DrawStart()
        {
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("The repair tool will help you repair AIs following class, property and field renames.\n\nIt is a multi step process.\n\nFirst select the AI(s) to repair.", SharedStyles.BuiltIn.wrappedText);
            EditorGUILayout.EndVertical();
            _listView.Render(StoredAIs.AIs);

            GUI.enabled = _listView.hasSelected;
            if (GUILayout.Button("Proceed"))
            {
                StartResolutionAsync(_listView.GetSelectedItems());
            }
        }

        private void DrawTypeFix()
        {
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Type Resolution", EditorStyling.Skinned.boldTitle);
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("The left hand side of the list shows all types that could not be resolved.\n\nThe right hand side is where you can provide the new type name.\nYou can also leave it blank in which case the element will be removed from the AI.", SharedStyles.BuiltIn.wrappedText);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUIUtility.labelWidth = 100f;
            _utility.hideApexTypes = EditorGUILayout.Toggle("Hide Apex types", _utility.hideApexTypes);
            EditorGUIUtility.labelWidth = 0f;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, "Box");

            var labelContent = new GUIContent();
            foreach (var entry in _typeResolution)
            {
                EditorGUILayout.BeginHorizontal("Box");
                labelContent.text = entry.unresolvedTypeName.fullTypeName;
                labelContent.tooltip = entry.unresolvedTypeName.assemblyName;
                EditorGUILayout.LabelField(labelContent);

                EditorGUILayout.LabelField("->", GUILayout.Width(40f));

                if (entry.resolvedTypeName != null)
                {
                    labelContent.text = entry.resolvedTypeName.fullTypeName;
                    labelContent.tooltip = entry.resolvedTypeName.assemblyName;
                }
                else
                {
                    labelContent.text = "?";
                    labelContent.tooltip = "Unresolved type";
                }

                EditorGUILayout.LabelField(labelContent);

                if (GUILayout.Button("...", EditorStyling.Skinned.fixedButton))
                {
                    var screenPos = EditorGUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                    GenericSelectorWindow.Show<TypeNameTokens>(
                        screenPos,
                        "Select Type",
                        _utility.GetAvailableTypes(entry.baseType),
                        RenderListItem,
                        MatchItem,
                        true,
                        false,
                        t =>
                        {
                            _utility.UpdateResolvedType(entry, t.Length > 0 ? t[0] : null);
                            this.Repaint();
                        });

                    this.Repaint();
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Back"))
            {
                ToStart();
            }

            if (GUILayout.Button("Proceed"))
            {
                _working = true;

                EditorAsync.Execute(
                    () =>
                    {
                        _memberResolution = _utility.IdentifyUnresolvedMembers();
                        if (_memberResolution.Length > 0)
                        {
                            _step = Step.Members;
                        }
                        else
                        {
                            _mismatchResolution = _utility.IdentifyMismatchedMembers();
                            if (_mismatchResolution.Length > 0)
                            {
                                _step = Step.Mismatch;
                            }
                            else
                            {
                                _status = _utility.GetStatus();
                                _step = Step.Summary;
                            }
                        }
                    },
                    () =>
                    {
                        _scrollPos = Vector2.zero;
                        EndWork();
                    });
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawPropFix()
        {
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Member Resolution", EditorStyling.Skinned.boldTitle);
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("For each type in which unresolved field or property references were found, you will see a section below.\n\nThe left hand side of the list shows all fields and properties that could not be resolved.\n\nThe right hand side is where you can provide the new name.\nYou can also leave it blank in which case the reference will be removed from the AI.", SharedStyles.BuiltIn.wrappedText);
            EditorGUILayout.EndVertical();

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, "Box");

            var labelContent = new GUIContent();
            foreach (var type in _memberResolution)
            {
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.BeginHorizontal(SharedStyles.BuiltIn.listItemHeader);
                labelContent.text = type.typeName.fullTypeName;
                labelContent.tooltip = type.typeName.assemblyName;
                EditorGUILayout.LabelField(labelContent);
                EditorGUILayout.EndHorizontal();

                labelContent.tooltip = string.Empty;
                foreach (var member in type.unresolvedMembers)
                {
                    EditorGUILayout.BeginHorizontal();
                    labelContent.text = member.unresolvedName;
                    EditorGUILayout.LabelField(labelContent);

                    EditorGUILayout.LabelField("->", GUILayout.Width(40f));

                    if (member.resolvedName != null)
                    {
                        labelContent.text = member.resolvedName;
                    }
                    else
                    {
                        labelContent.text = "?";
                    }

                    EditorGUILayout.LabelField(labelContent);

                    if (GUILayout.Button("...", EditorStyling.Skinned.fixedButton))
                    {
                        var screenPos = EditorGUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                        GenericSelectorWindow.Show<string>(
                            screenPos,
                            "Select Member",
                            type.potentialReplacements,
                            RenderListItem,
                            MatchItem,
                            true,
                            false,
                            m =>
                            {
                                if (m.Length > 0)
                                {
                                    member.resolvedName = m[0];
                                }
                                else
                                {
                                    member.resolvedName = null;
                                }

                                this.Repaint();
                            });

                        this.Repaint();
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Back"))
            {
                if (_typeResolution.Length == 0)
                {
                    ToStart();
                }
                else
                {
                    _scrollPos = Vector2.zero;
                    _step = Step.Types;
                }
            }

            if (GUILayout.Button("Proceed"))
            {
                _working = true;

                EditorAsync.Execute(
                    () =>
                    {
                        _mismatchResolution = _utility.IdentifyMismatchedMembers();
                        if (_mismatchResolution.Length > 0)
                        {
                            _step = Step.Mismatch;
                        }
                        else
                        {
                            _status = _utility.GetStatus();
                            _step = Step.Summary;
                        }
                    },
                    () =>
                    {
                        _scrollPos = Vector2.zero;
                        EndWork();
                    });
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawMismatchFix()
        {
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Mismatch Resolution", EditorStyling.Skinned.boldTitle);
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("For each type in which a mismatch between a field's or property's type and the actual value was found, you will see a section below.\n\nThe left hand side of the list shows all fields and properties with mismatched values.\n\nThe right hand side is where you can provide a new value if applicable.\nYou can also leave it blank in which case the value will be reset to its default value.", SharedStyles.BuiltIn.wrappedText);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUIUtility.labelWidth = 100f;
            _utility.hideApexTypes = EditorGUILayout.Toggle("Hide Apex types", _utility.hideApexTypes);
            EditorGUIUtility.labelWidth = 0f;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, "Box");

            var labelContent = new GUIContent();
            foreach (var mm in _mismatchResolution)
            {
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.BeginHorizontal(SharedStyles.BuiltIn.listItemHeader);
                labelContent.text = mm.parentName;
                EditorGUILayout.LabelField(labelContent);
                EditorGUILayout.EndHorizontal();

                labelContent.tooltip = string.Empty;
                foreach (var member in mm.mismatchedMembers)
                {
                    EditorGUILayout.BeginHorizontal();
                    labelContent.text = member.item.name;
                    if (string.IsNullOrEmpty(labelContent.text))
                    {
                        labelContent.text = "Item";
                    }

                    EditorGUILayout.LabelField(labelContent);

                    labelContent.text = member.typeName;
                    EditorGUILayout.LabelField(labelContent);

                    if (member.isCorrectable)
                    {
                        EditorGUILayout.LabelField("->", GUILayout.Width(40f));

                        if (member.baseType.IsEnum)
                        {
                            var val = member.resolvedValue;
                            if (val == null)
                            {
                                var arr = Enum.GetValues(member.baseType);
                                if (arr.Length > 0)
                                {
                                    val = arr.GetValue(0);
                                }
                            }

                            if (val != null)
                            {
                                member.resolvedValue = EditorGUILayout.EnumPopup((Enum)val);
                            }
                        }
                        else
                        {
                            if (member.resolvedTypeName != null)
                            {
                                labelContent.text = member.resolvedTypeName.fullTypeName;
                            }
                            else
                            {
                                labelContent.text = "?";
                            }

                            EditorGUILayout.LabelField(labelContent);

                            if (GUILayout.Button("...", EditorStyling.Skinned.fixedButton))
                            {
                                var screenPos = EditorGUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                                GenericSelectorWindow.Show<TypeNameTokens>(
                                    screenPos,
                                    "Select Type",
                                    _utility.GetAvailableTypes(member.baseType),
                                    RenderListItem,
                                    MatchItem,
                                    true,
                                    false,
                                    t =>
                                    {
                                        member.resolvedTypeName = t.Length > 0 ? t[0] : null;
                                        this.Repaint();
                                    });

                                this.Repaint();
                            }
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Back"))
            {
                if (_memberResolution.Length > 0)
                {
                    _step = Step.Members;
                }
                else if (_typeResolution.Length > 0)
                {
                    _step = Step.Types;
                }
                else
                {
                    ToStart();
                }
            }

            if (GUILayout.Button("Proceed"))
            {
                _working = true;

                EditorAsync.Execute(
                    () => _utility.GetStatus(),
                    (rs) =>
                    {
                        _status = rs;
                        _scrollPos = Vector2.zero;
                        _step = Step.Summary;

                        EndWork();
                    });
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawSummary()
        {
            var summary = new StringBuilder("You can now finalize the repair.");

            if (_utility.customRepairsMade)
            {
                summary.AppendLine();
                summary.AppendLine();
                summary.Append("Custom repairs are ready to be applied.");
            }

            summary.AppendLine();
            summary.AppendLine();
            if (_status.unresolvedTypesCount == 0)
            {
                summary.Append("No unresolved types were detected.");
            }
            else
            {
                summary.AppendFormat("{0}/{1} unresolved types have been resolved.", _status.resolvedTypesCount, _status.unresolvedTypesCount);
            }

            summary.AppendLine();
            summary.AppendLine();
            if (_status.unresolvedMembersCount == 0)
            {
                summary.Append("No unresolved members were detected.");
            }
            else
            {
                summary.AppendFormat("{0}/{1} unresolved members have been resolved.", _status.resolvedMembersCount, _status.unresolvedMembersCount);
            }

            summary.AppendLine();
            summary.AppendLine();
            if (_status.unresolvedMismatchesCount == 0)
            {
                summary.Append("No mismatches were detected.");
            }
            else
            {
                if (_status.resolvableMismatchesCount > 0)
                {
                    summary.AppendFormat("{0}/{1} mismatches have been resolved.", _status.resolvedMismatchesCount, _status.resolvableMismatchesCount);
                }

                if (_status.unresolvedMismatchesCount > _status.resolvableMismatchesCount)
                {
                    if (_status.resolvableMismatchesCount > 0)
                    {
                        summary.Append(" An additional ");
                    }

                    summary.AppendFormat("{0} unresolvable mismatches will be corrected to their default value.", _status.unresolvedMismatchesCount - _status.resolvableMismatchesCount);
                }
            }

            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Summary", EditorStyling.Skinned.boldTitle);
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField(summary.ToString(), SharedStyles.BuiltIn.wrappedText);
            EditorGUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Back"))
            {
                if (_status.unresolvedMismatchesCount > 0)
                {
                    _step = Step.Mismatch;
                }
                else if (_status.unresolvedMembersCount > 0)
                {
                    _step = Step.Members;
                }
                else if (_status.unresolvedTypesCount > 0)
                {
                    _step = Step.Types;
                }
                else
                {
                    ToStart();
                }
            }

            if (GUILayout.Button("Complete"))
            {
                var message = new StringBuilder();
                if (_status.unresolvedTypesCount > _status.resolvedTypesCount)
                {
                    message.AppendLine("There are still unresolved types, these will be removed if you complete the repair.");
                }

                if (_status.unresolvedMembersCount > _status.resolvedMembersCount)
                {
                    message.AppendLine();
                    message.AppendLine("There are still unresolved members, these will be removed if you complete the repair.");
                }

                if (_status.resolvableMismatchesCount > _status.resolvedMismatchesCount)
                {
                    message.AppendLine();
                    message.AppendLine("There are still unresolved mismatches, these will be removed if you complete the repair.");
                }

                message.AppendLine();
                message.AppendLine("Do you wish to apply the repair?");

                if (EditorUtility.DisplayDialog("Confirm Repair", message.ToString(), "Yes", "No"))
                {
                    _working = true;

                    EditorAsync.Execute(
                        _utility.ExecuteRepairs,
                        () =>
                        {
                            _utility.SaveChanges();
                            Debug.Log("Repair Complete.");
                            if (EditorUtility.DisplayDialog("Repair Complete", "The selected AIs were successfully repaired.\n\nDo you wish to load the repaired AIs?", "Yes", "No"))
                            {
                                foreach (var aiId in _utility.repairedAIIds)
                                {
                                    AIEditorWindow.Open(aiId);
                                }
                            }

                            this.Close();
                        });
                }
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}
