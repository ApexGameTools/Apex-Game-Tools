namespace Apex.AI.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Apex.AI.Serialization;
    using Apex.Serialization;
    using UnityEditor;
    using Utilities;

    internal sealed class RepairUtility
    {
        private bool _hideApexTypes = true;
        private Type[] _availableTypes;
        private TypeNameTokens[] _availableTypeNames;

        private AIStorage[] _ais;
        private string[] _repairedAIIds;
        private StageElement[] _stagedAis;
        private StageElement[] _stagedEditorConfigs;
        private IRepairTask[] _customTasks;

        private Dictionary<string, Type> _identifiedTypes;
        private UnresolvedType[] _typeResolution;
        private MemberResolutionInfo[] _membersResolution;
        private MismatchedMember[] _mismatchedMembers;
        private ResolutionStatus _status;

        private Type[] availableTypes
        {
            get
            {
                if (_availableTypes == null)
                {
                    _availableTypes = (from t in ApexReflection.GetRelevantTypes()
                                       where !t.IsAbstract && !t.IsDefined<HiddenAttribute>(false) && t.GetConstructor(Type.EmptyTypes) != null && (!_hideApexTypes || t.Namespace == null || !t.Namespace.StartsWith("Apex.AI")) && (t.IsDefined<ApexSerializedTypeAttribute>(true) || SerializationMaster.GetSerializedProperties(t).Any() || SerializationMaster.GetSerializedFields(t).Any())
                                       orderby t.FullName
                                       select t).ToArray();
                }

                return _availableTypes;
            }
        }

        internal TypeNameTokens[] availableTypesNames
        {
            get
            {
                if (_availableTypeNames == null)
                {
                    _availableTypeNames = (from t in this.availableTypes
                                           select new TypeNameTokens(t.AssemblyQualifiedName)).ToArray();
                }

                return _availableTypeNames;
            }
        }

        internal bool hideApexTypes
        {
            get
            {
                return _hideApexTypes;
            }

            set
            {
                if (value != _hideApexTypes)
                {
                    _hideApexTypes = value;
                    _availableTypes = null;
                    _availableTypeNames = null;
                    AutoResolveTypes();
                }
            }
        }

        internal bool customRepairsMade
        {
            get;
            set;
        }

        internal AIStorage[] ais
        {
            get { return _ais; }
        }

        internal string[] repairedAIIds
        {
            get { return _repairedAIIds ?? Empty<string>.array; }
        }

        internal TypeNameTokens[] GetAvailableTypes(Type baseType)
        {
            if (baseType == null)
            {
                return this.availableTypesNames;
            }

            return (from t in this.availableTypes
                    where baseType.IsAssignableFrom(t)
                    select new TypeNameTokens(t.AssemblyQualifiedName)).ToArray();
        }

        internal void Begin(AIStorage[] ais)
        {
            _ais = ais;
            _repairedAIIds = null;
            _identifiedTypes = new Dictionary<string, Type>();
            _stagedAis = new StageElement[ais.Length];
            for (int i = 0; i < ais.Length; i++)
            {
                _stagedAis[i] = SerializationMaster.Deserialize(ais[i].configuration);
            }

            if (_customTasks == null)
            {
                _customTasks = (from t in ApexReflection.GetRelevantTypes()
                                where !t.IsAbstract && typeof(IRepairTask).IsAssignableFrom(t) && t.GetConstructor(Type.EmptyTypes) != null
                                select Activator.CreateInstance(t) as IRepairTask).OrderByDescending(t => t.versionThreshold).ToArray();
            }

            if (_customTasks.Length > 0)
            {
                for (int i = 0; i < ais.Length; i++)
                {
                    var aiVersion = new AIVersion(ais[i].version).ToVersion();
                    for (int j = 0; j < _customTasks.Length; j++)
                    {
                        var task = _customTasks[j];
                        if (task.versionThreshold >= aiVersion)
                        {
                            var editorCfg = task.repairsEditorConfiguration ? GetEditorDoc(i) : null;
                            if (task.Repair(_stagedAis[i], editorCfg))
                            {
                                this.customRepairsMade = true;
                            }
                        }
                    }
                }
            }
        }

        internal UnresolvedType[] IdentifyUnresolvedTypes()
        {
            _typeResolution = (from s in DoIdentifyUnresolvedTypes()
                               orderby s.unresolvedTypeName.fullTypeName
                               select s).ToArray();

            AutoResolveTypes();

            return _typeResolution;
        }

        internal MemberResolutionInfo[] IdentifyUnresolvedMembers()
        {
            _membersResolution = DoIdentifyUnresolvedMembers().ToArray();

            return _membersResolution;
        }

        internal MismatchedMemberInfo[] IdentifyMismatchedMembers()
        {
            _mismatchedMembers = DoIdentifyMismatchedMembers().ToArray();
            return (from mm in _mismatchedMembers
                    orderby mm.parentName
                    group mm by mm.parentName into grouped
                    select new MismatchedMemberInfo
                    {
                        parentName = grouped.Key,
                        mismatchedMembers = (from mmItem in grouped
                                             orderby mmItem.item.name
                                             select mmItem).ToArray()
                    }).ToArray();
        }

        internal ResolutionStatus GetStatus()
        {
            if (_typeResolution.Length > 0 || _membersResolution.Length > 0 || _mismatchedMembers.Length > 0)
            {
                _status = new ResolutionStatus
                {
                    unresolvedTypesCount = _typeResolution.Length,
                    resolvedTypesCount = _typeResolution.Count(t => t.resolvedTypeName != null),
                    unresolvedMembersCount = _membersResolution.Select(mr => mr.unresolvedMembers.Count).Sum(),
                    resolvedMembersCount = _membersResolution.Select(mr => mr.unresolvedMembers.Count(m => m.resolvedName != null)).Sum(),
                    unresolvedMismatchesCount = _mismatchedMembers.Length,
                    resolvableMismatchesCount = _mismatchedMembers.Count(mm => mm.isCorrectable),
                    resolvedMismatchesCount = _mismatchedMembers.Count(mm => mm.resolvedTypeName != null || mm.resolvedValue != null)
                };
            }
            else
            {
                _status = new ResolutionStatus();
            }

            return _status;
        }

        internal void ExecuteRepairs()
        {
            RecordRepairedAIs();

            HashSet<string> unresolvedLookup = null;
            var hasUnresolvedTypes = _status.resolvedTypesCount != _status.unresolvedTypesCount;
            if (hasUnresolvedTypes)
            {
                var unresolved = from urt in _typeResolution
                                 where urt.resolvedTypeName == null
                                 select urt.unresolvedTypeName.completeTypeName;

                unresolvedLookup = new HashSet<string>();
                foreach (var urtn in unresolved)
                {
                    unresolvedLookup.Add(urtn);
                }
            }

            if (hasUnresolvedTypes)
            {
                //Due to the separation of ai and editor data, the first thing we need to do is to handle the core elements of the ai
                //and ensure that any removals are also reflected in the editor data
                for (int i = 0; i < _stagedAis.Length; i++)
                {
                    RepairCoreElements(i, unresolvedLookup);
                }

                //Correct any additional type issues
                foreach (var urt in _typeResolution.Where(t => t.resolvedTypeName == null))
                {
                    foreach (var el in urt.elements)
                    {
                        el.Remove();
                    }
                }
            }

            //Next we handle members
            var hasUnresolvedMembers = _status.resolvedMembersCount != _status.unresolvedMembersCount;
            if (hasUnresolvedMembers)
            {
                foreach (var mr in _membersResolution)
                {
                    foreach (var mm in mr.unresolvedMembers.Where(um => um.resolvedName == null))
                    {
                        foreach (var el in mm.items)
                        {
                            el.Remove();
                        }
                    }
                }
            }

            //And finally mismatches
            foreach (var mm in _mismatchedMembers.Where(m => m.resolvedTypeName == null && m.resolvedValue == null))
            {
                mm.item.Remove();
            }

            for (int i = 0; i < _ais.Length; i++)
            {
                _ais[i].configuration = SerializationMaster.Serialize(_stagedAis[i]);

                if (_stagedEditorConfigs != null && _stagedEditorConfigs[i] != null)
                {
                    _ais[i].editorConfiguration = SerializationMaster.Serialize(_stagedEditorConfigs[i]);
                }
            }
        }

        private void RecordRepairedAIs()
        {
            var repaired = new List<StageElement>();
            for (int i = 0; i < _typeResolution.Length; i++)
            {
                var tre = _typeResolution[i].elements;
                for (int j = 0; j < tre.Length; j++)
                {
                    StageItem el = tre[j];
                    while (el.parent != null)
                    {
                        el = el.parent;
                    }

                    var root = el as StageElement;
                    repaired.AddUnique(root);
                }
            }

            for (int i = 0; i < _membersResolution.Length; i++)
            {
                var mre = _membersResolution[i].unresolvedMembers;
                for (int j = 0; j < mre.Count; j++)
                {
                    var items = mre[j].items;

                    for (int k = 0; k < items.Length; k++)
                    {
                        StageItem el = items[k];
                        while (el.parent != null)
                        {
                            el = el.parent;
                        }

                        var root = el as StageElement;
                        repaired.AddUnique(root);
                    }
                }
            }

            for (int i = 0; i < _mismatchedMembers.Length; i++)
            {
                var el = _mismatchedMembers[i].item;
                while (el.parent != null)
                {
                    el = el.parent;
                }

                var root = el as StageElement;
                repaired.AddUnique(root);
            }

            var ids = repaired.Select(el => el.ValueOrDefault("_id", string.Empty));

            //Since the serialization uses a compact (N) format for guids but the AIStorage component does not, we have to do this rather lame conversion.
            _repairedAIIds = ids.Where(id => !string.IsNullOrEmpty(id)).Select(id => new Guid(id).ToString()).ToArray();
        }

        internal void SaveChanges()
        {
            var aiVersion = AIVersion.FromVersion(this.GetType().Assembly.GetName().Version);
            foreach (var ai in _ais)
            {
                ai.version = aiVersion.version;
                EditorUtility.SetDirty(ai);
            }

            AssetDatabase.SaveAssets();
        }

        internal void UpdateResolvedType(UnresolvedType t, TypeNameTokens typeName)
        {
            if (typeName != null && !_identifiedTypes.ContainsKey(typeName.completeTypeName))
            {
                _identifiedTypes[typeName.completeTypeName] = Type.GetType(typeName.completeTypeName);
            }
            else if (t.resolvedTypeName != null && typeName == null)
            {
                _identifiedTypes.Remove(t.resolvedTypeName.completeTypeName);
            }

            t.resolvedTypeName = typeName;
        }

        private void AutoResolveTypes()
        {
            foreach (var urt in _typeResolution)
            {
                if (urt.resolvedTypeName != null)
                {
                    continue;
                }

                var bestBet = this.availableTypesNames.FirstOrDefault(ti => ti.simpleTypeName.Equals(urt.unresolvedTypeName.simpleTypeName, StringComparison.OrdinalIgnoreCase));
                if (bestBet != null)
                {
                    UpdateResolvedType(urt, bestBet);
                }
            }
        }

        private IEnumerable<UnresolvedType> DoIdentifyUnresolvedTypes()
        {
            foreach (var doc in _stagedAis)
            {
                _identifiedTypes[doc.AttributeValue<string>("type")] = typeof(UtilityAI);

                var types = from el in doc.Descendants<StageElement>()
                            let type = el.Attribute("type")
                            where type != null
                            group el by type.value into typeGroup
                            select new
                            {
                                elements = typeGroup.ToArray(),
                                type = typeGroup.Key
                            };

                foreach (var t in types)
                {
                    var type = Type.GetType(t.type, false);
                    if (type == null)
                    {
                        yield return new UnresolvedType
                        {
                            elements = t.elements,
                            unresolvedTypeName = new TypeNameTokens(t.type),
                            baseType = ResolveExpectedBaseType(t.elements[0], true)
                        };
                    }
                    else
                    {
                        _identifiedTypes[t.type] = type;
                    }
                }
            }
        }

        private IEnumerable<MemberResolutionInfo> DoIdentifyUnresolvedMembers()
        {
            //Since not everything is serialized, e.g. null values, we cannot simply get the first element of a specific type and use that,
            //we have to get the union of all fields that are serialized across all elements of that type.
            var typeElements = from doc in _stagedAis
                               from el in doc.Descendants<StageElement>()
                               let type = el.Attribute("type")
                               where type != null
                               group el by type.value into typeGroup
                               select new
                               {
                                   type = typeGroup.Key,
                                   members = (from el in typeGroup
                                              from m in el.Items()
                                              let name = m.name
                                              orderby name
                                              group m by m.name into memberGroup
                                              select new
                                              {
                                                  name = memberGroup.Key,
                                                  items = memberGroup.ToArray()
                                              })
                               };

            //Next we check that each member found actually exists on the type.
            MemberResolutionInfo memberInfo = new MemberResolutionInfo();
            foreach (var t in typeElements.OrderBy(t => t.type))
            {
                var typeName = t.type;
                var type = Type.GetType(typeName, false);

                //This can still happen since the user can have opted to not resolve a type and it will be removed later.
                if (type == null)
                {
                    continue;
                }

                //Check fields and properties
                var fieldsAndPropNames = SerializationMaster.GetSerializedFields(type).Select(f => f.Name).Concat(SerializationMaster.GetSerializedProperties(type).Select(p => p.Name)).ToArray();
                foreach (var m in t.members)
                {
                    if (!fieldsAndPropNames.Contains(m.name, StringComparer.Ordinal))
                    {
                        memberInfo.unresolvedMembers.Add(
                            new Member
                            {
                                unresolvedName = m.name,
                                items = m.items
                            });
                    }
                    else
                    {
                        memberInfo.validMappedMembers.Add(m.name);
                    }
                }

                //There is no need to new up various instances if no unresolved members are found, so we do it this way
                if (memberInfo.unresolvedMembers.Count > 0)
                {
                    memberInfo.type = type;
                    memberInfo.typeName = new TypeNameTokens(typeName);
                    yield return memberInfo;

                    memberInfo = new MemberResolutionInfo();
                }
                else
                {
                    memberInfo.validMappedMembers.Clear();
                }
            }
        }

        private IEnumerable<MismatchedMember> DoIdentifyMismatchedMembers()
        {
            var allItems = from doc in _stagedAis
                           from item in doc.Descendants()
                           select item;

            foreach (var item in allItems)
            {
                var expectedType = ResolveExpectedBaseType(item, true);
                if (expectedType == null)
                {
                    //Missing fields are handled by member resolution
                    continue;
                }

                //First handle typed elements
                string typeName = null;
                if (item is StageElement)
                {
                    var element = (StageElement)item;
                    typeName = element.AttributeValueOrDefault<string>("type");
                }

                if (typeName != null)
                {
                    //If the type does not match we register it. In case the type is invalid we just ignore it as it will already have been handled by type resolution.
                    var type = Type.GetType(typeName, false);
                    if (type != null && !expectedType.IsAssignableFrom(type))
                    {
                        yield return new MismatchedMember
                        {
                            parentName = string.Format("{0}({1})", item.parent.name, GetParentTypeName(item)),
                            baseType = expectedType,
                            item = item,
                            typeName = type.FullName
                        };
                    }
                }
                else if (item is StageList)
                {
                    if (expectedType.IsGenericType)
                    {
                        var expectedDef = expectedType.GetGenericTypeDefinition();
                        if (expectedDef != typeof(List<>) && expectedDef != typeof(Dictionary<,>))
                        {
                            yield return new MismatchedMember
                            {
                                parentName = string.Format("{0} ({1})", item.parent.name, GetParentTypeName(item)),
                                item = item,
                                typeName = "List"
                            };
                        }
                    }
                    else if (!expectedType.IsArray)
                    {
                        yield return new MismatchedMember
                        {
                            parentName = string.Format("{0} ({1})", item.parent.name, GetParentTypeName(item)),
                            item = item,
                            typeName = "List"
                        };
                    }
                }
                else
                {
                    //Untyped elements or simple values can be produced by custom stagers or converters. Here we can only try to unstage.
                    object value = null;
                    try
                    {
                        value = SerializationMaster.Unstage(item, expectedType);
                    }
                    catch
                    {
                    }

                    if (value == null || !expectedType.IsAssignableFrom(value.GetType()))
                    {
                        string tn = "Unknown";
                        if (value != null)
                        {
                            tn = value.GetType().FullName;
                        }
                        else if (item is StageValue)
                        {
                            tn = ((StageValue)item).value;
                        }

                        //For enums we want to be able to display a drop down for selection.
                        var bt = expectedType.IsEnum ? expectedType : null;

                        yield return new MismatchedMember
                        {
                            parentName = string.Format("{0} ({1})", item.parent.name, GetParentTypeName(item)),
                            item = item,
                            typeName = tn,
                            baseType = bt
                        };
                    }
                }
            }
        }

        private void RepairCoreElements(int docIdx, HashSet<string> unresolvedLookup)
        {
            var doc = _stagedAis[docIdx];
            var editorDoc = GetEditorDoc(docIdx);
            var _removedSelectorIds = new List<string>();

            //Since we need to update selector links we process all selectors first and then the qualifiers and actions afterwards
            var selectors = doc.List("_selectors").Elements().ToArray();
            var selectorViews = editorDoc.Elements(GuiSerializer.ElementName.SelectorView).ToArray();
            for (int i = 0; i < selectors.Length; i++)
            {
                var type = selectors[i].AttributeValue<string>("type");
                if (unresolvedLookup.Contains(type))
                {
                    _removedSelectorIds.Add(selectors[i].Value<string>("_id"));
                    selectors[i].Remove();
                    selectorViews[i].Remove();
                    continue;
                }
            }

            selectors = doc.List("_selectors").Elements().ToArray();
            selectorViews = editorDoc.Elements(GuiSerializer.ElementName.SelectorView).ToArray();
            for (int i = 0; i < selectors.Length; i++)
            {
                var qualifiers = selectors[i].List("_qualifiers").Elements().ToArray();
                var qualifierViews = selectorViews[i].Elements(GuiSerializer.ElementName.QualifierView).ToArray();
                for (int j = 0; j < qualifiers.Length; j++)
                {
                    var type = qualifiers[j].AttributeValue<string>("type");
                    if (unresolvedLookup.Contains(type))
                    {
                        qualifiers[j].Remove();
                        qualifierViews[j].Remove();
                        continue;
                    }

                    var action = qualifiers[j].Element("action");
                    var actionView = qualifierViews[j].Element(GuiSerializer.ElementName.ActionView);
                    if (action != null)
                    {
                        type = action.AttributeValue<string>("type");
                        var selectorId = action.ValueOrDefault<string>("_selectorId");
                        if (unresolvedLookup.Contains(type) || (selectorId != null && _removedSelectorIds.Contains(selectorId, StringComparer.Ordinal)))
                        {
                            action.Remove();
                            actionView.Remove();
                        }
                    }
                }
            }
        }

        private string GetParentTypeName(StageItem item)
        {
            var parent = item.parent;

            if (parent is StageElement)
            {
                var parentTypeName = ((StageElement)parent).AttributeValueOrDefault<string>("type");
                return parentTypeName != null ? parentTypeName : string.Empty;
            }

            var listType = ResolveExpectedBaseType(item, false);
            return listType != null ? listType.ProperName(false) : "List";
        }

        private Type ResolveExpectedBaseType(StageItem el, bool elementType)
        {
            var parent = el.parent;
            if (parent == null)
            {
                return null;
            }

            var parentElement = parent as StageElement;
            if (parentElement != null)
            {
                var parentTypeName = parentElement.AttributeValueOrDefault<string>("type");
                if (parentTypeName == null)
                {
                    /* For custom types that have no type attribute, the entire type will be validated at once, so we ignore individual members. */
                    return null;
                }

                Type parentType;
                if (!_identifiedTypes.TryGetValue(parentTypeName, out parentType))
                {
                    return null;
                }

                var prop = parentType.GetProperty(el.name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (prop != null)
                {
                    return prop.PropertyType;
                }

                var field = parentType.GetField(el.name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field != null)
                {
                    return field.FieldType;
                }

                return null;
            }

            var parentList = parent as StageList;
            if (parentList == null)
            {
                return null;
            }

            var listType = ResolveExpectedBaseType(parentList, elementType);
            if (!elementType)
            {
                return listType;
            }

            while (typeof(IList).IsAssignableFrom(listType))
            {
                if (listType.IsArray)
                {
                    listType = listType.GetElementType();
                }
                else if (listType.IsGenericType)
                {
                    listType = listType.GetGenericArguments()[0];
                }
                else
                {
                    listType = null;
                }
            }

            return listType;
        }

        private StageElement GetEditorDoc(int index)
        {
            if (_stagedEditorConfigs == null)
            {
                _stagedEditorConfigs = new StageElement[_ais.Length];
            }

            if (_stagedEditorConfigs[index] == null)
            {
                _stagedEditorConfigs[index] = SerializationMaster.Deserialize(_ais[index].editorConfiguration);
            }

            return _stagedEditorConfigs[index];
        }

        internal struct ResolutionStatus
        {
            internal int unresolvedTypesCount;
            internal int resolvedTypesCount;
            internal int unresolvedMembersCount;
            internal int resolvedMembersCount;
            internal int unresolvedMismatchesCount;
            internal int resolvableMismatchesCount;
            internal int resolvedMismatchesCount;
        }

        internal class UnresolvedType
        {
            private TypeNameTokens _resolvedTypeName;

            internal StageElement[] elements;
            internal TypeNameTokens unresolvedTypeName;
            internal Type baseType;

            internal TypeNameTokens resolvedTypeName
            {
                get
                {
                    return _resolvedTypeName;
                }

                set
                {
                    _resolvedTypeName = value;
                    if (_resolvedTypeName != null)
                    {
                        foreach (var element in elements)
                        {
                            element.SetTextAttribute("type", _resolvedTypeName.completeTypeName);
                        }
                    }
                    else
                    {
                        foreach (var element in elements)
                        {
                            element.SetTextAttribute("type", unresolvedTypeName.completeTypeName);
                        }
                    }
                }
            }
        }

        internal class MismatchedMember
        {
            private TypeNameTokens _resolvedTypeName;
            private object _resolvedValue;

            internal string parentName;
            internal StageItem item;
            internal string typeName;
            internal Type baseType;

            internal bool isCorrectable
            {
                get { return this.baseType != null; }
            }

            internal TypeNameTokens resolvedTypeName
            {
                get
                {
                    return _resolvedTypeName;
                }

                set
                {
                    //We only set the type if resolved is not null, as the element is deleted otherwise so no need to reset it to its original value.
                    _resolvedTypeName = value;
                    if (_resolvedTypeName != null)
                    {
                        ((StageElement)item).SetTextAttribute("type", _resolvedTypeName.completeTypeName);
                    }
                }
            }

            /// <summary>
            /// Gets or sets the resolved value, this is for value types, currently only implemented for enums.
            /// </summary>
            internal object resolvedValue
            {
                get
                {
                    return _resolvedValue;
                }

                set
                {
                    if (_resolvedValue == value)
                    {
                        return;
                    }

                    _resolvedValue = value;
                    if (_resolvedValue != null)
                    {
                        ((StageValue)item).value = SerializationMaster.ToString(_resolvedValue);
                    }
                }
            }
        }

        internal class MismatchedMemberInfo
        {
            internal string parentName;
            internal MismatchedMember[] mismatchedMembers;
        }

        internal class Member
        {
            private string _resolvedName;

            internal StageItem[] items;
            internal string unresolvedName;

            internal string resolvedName
            {
                get
                {
                    return _resolvedName;
                }

                set
                {
                    _resolvedName = value;
                    if (_resolvedName != null)
                    {
                        foreach (var item in items)
                        {
                            item.name = _resolvedName;
                        }
                    }
                    else
                    {
                        foreach (var item in items)
                        {
                            item.name = unresolvedName;
                        }
                    }
                }
            }
        }

        internal class MemberResolutionInfo
        {
            internal Type type;
            internal TypeNameTokens typeName;
            internal List<Member> unresolvedMembers = new List<Member>();
            internal HashSet<string> validMappedMembers = new HashSet<string>();

            internal IEnumerable<string> potentialReplacements
            {
                get
                {
                    var fields = from f in SerializationMaster.GetSerializedFields(this.type)
                                 where !this.validMappedMembers.Contains(f.Name) && !this.unresolvedMembers.Any(um => um.resolvedName == f.Name)
                                 orderby f.Name
                                 select f.Name;

                    var props = from p in SerializationMaster.GetSerializedProperties(this.type)
                                where !this.validMappedMembers.Contains(p.Name) && !this.unresolvedMembers.Any(um => um.resolvedName == p.Name)
                                orderby p.Name
                                select p.Name;

                    return fields.Concat(props);
                }
            }
        }
    }
}
