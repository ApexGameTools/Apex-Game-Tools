/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Apex.AI.Serialization;
    using Apex.Serialization;
    using Utilities;

    internal static class ReflectMaster
    {
        private static readonly Dictionary<Type, Type> _typedEditorFields = new Dictionary<Type, Type>();
        private static readonly DependencyChecker _noDependencies = new DependencyChecker();
        private static bool _isInitialized;

        internal static EditorItem Reflect(object item)
        {
            if (item == null)
            {
                return null;
            }

            var itemType = item.GetType();

            DependencyChecker dependencyChecker;
            var categories = GetSerializedMembers(itemType, item, out dependencyChecker);

            return new EditorItem
            {
                item = item,
                name = DisplayHelper.GetFriendlyName(itemType),
                fieldCategories = categories,
                dependencyChecker = dependencyChecker ?? _noDependencies
            };
        }

        private static EditorFieldCategory[] GetSerializedMembers(Type t, object item, out DependencyChecker dependencyChecker)
        {
            var props = from p in t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        where p.CanRead && p.CanWrite
                        select (MemberInfo)p;

            var fields = t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Cast<MemberInfo>();

            var members = (from m in props.Concat(fields)
                           let attribSerialize = m.GetAttribute<ApexSerializationAttribute>(true)
                           where attribSerialize != null && !attribSerialize.hideInEditor
                           select new
                           {
                               member = m,
                               attribName = m.GetAttribute<FriendlyNameAttribute>(true),
                               attribCategory = m.GetAttribute<MemberCategoryAttribute>(true),
                               memberEdit = m.GetAttribute<MemberEditorAttribute>(true),
                               dependencies = m.GetAttributes<MemberDependencyAttribute>(true),
                           }).ToArray();

            var memberData = from m in members
                             select new MemberData
                             {
                                 member = m.member,
                                 rendererType = (m.memberEdit != null) ? m.memberEdit.GetType() : null,
                                 name = (m.attribName != null && !string.IsNullOrEmpty(m.attribName.name)) ? m.attribName.name : m.member.Name.ExpandFromPascal(),
                                 description = m.attribName != null ? m.attribName.description : null,
                                 category = m.attribCategory != null ? m.attribCategory.name : string.Empty,
                                 outerSortOrder = m.attribCategory != null ? m.attribCategory.sortOrder : 0,
                                 innerSortOrder = m.attribName != null ? m.attribName.sortOrder : 0
                             };

            var result = (from m in memberData
                          orderby m.outerSortOrder, m.category
                          group m by m.category into categorized
                          select new EditorFieldCategory
                          {
                              name = categorized.Key,
                              fields = (from f in categorized
                                        let ef = GetEditorField(f, item)
                                        where ef != null
                                        orderby f.innerSortOrder, f.name
                                        select ef).ToArray()
                          }).ToArray();

            dependencyChecker = null;
            var membersWithDependencies = members.Where(m => m.dependencies != null);
            if (membersWithDependencies.Any())
            {
                dependencyChecker = new DependencyChecker();

                var lookup = (from cat in result
                              from f in cat.fields
                              select f).ToDictionary(f => f.memberName);

                foreach (var mwd in membersWithDependencies)
                {
                    dependencyChecker.Add(
                        mwd.member.Name,
                        (from d in mwd.dependencies
                         let dependee = lookup.Value(d.dependsOn)
                         where dependee != null
                         select new DependencyCheck(dependee, d.value, d.match, d.compare, d.isMask)).ToArray());
                }
            }

            return result;
        }

        private static IEditorField GetEditorField(MemberData member, object owner)
        {
            if (!_isInitialized)
            {
                lock (_typedEditorFields)
                {
                    if (!_isInitialized)
                    {
                        PopulateKnownEditorFields();
                        _isInitialized = true;
                    }
                }
            }

            var forType = GetMemberType(member);
            if (forType.IsGenericType)
            {
                forType = forType.GetGenericTypeDefinition();
            }
            else if (forType.IsEnum)
            {
                if (forType.IsDefined<FlagsAttribute>(false))
                {
                    return new EnumMaskField(member, owner);
                }

                return new EnumSelectField(member, owner);
            }
            else if (forType.IsArray)
            {
                forType = typeof(Array);
            }

            Type result = null;
            if (_typedEditorFields.TryGetValue(forType, out result))
            {
                return Activator.CreateInstance(result, member, owner) as IEditorField;
            }

            return new CustomField(member, owner);
        }

        private static Type GetMemberType(MemberData member)
        {
            if (member.rendererType != null)
            {
                return member.rendererType;
            }

            var prop = member.member as PropertyInfo;
            if (prop != null)
            {
                return prop.PropertyType;
            }

            return ((FieldInfo)member.member).FieldType;
        }

        private static void PopulateKnownEditorFields()
        {
            var relevantTypes = ApexReflection.GetRelevantTypes();

            var editorFields = from t in relevantTypes
                               let tha = t.GetAttribute<TypesHandledAttribute>(true)
                               where tha != null && typeof(IEditorField).IsAssignableFrom(t) && !t.IsAbstract && t.GetConstructor(new Type[] { typeof(MemberData), typeof(object) }) != null
                               select new
                               {
                                   fieldType = t,
                                   typesHandled = tha.typesHandled
                               };

            foreach (var field in editorFields)
            {
                foreach (var t in field.typesHandled)
                {
                    _typedEditorFields[t] = field.fieldType;
                }
            }
        }
    }
}