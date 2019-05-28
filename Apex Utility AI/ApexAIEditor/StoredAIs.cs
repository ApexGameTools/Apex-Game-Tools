/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Apex.AI.Serialization;
    using Apex.DataStructures;
    using UnityEngine;

    internal static class StoredAIs
    {
        private static SafeDynamicArray<AIStorage> _ais;

        internal static IDynamicArray<AIStorage> AIs
        {
            get
            {
                if (_ais == null)
                {
                    _ais = new SafeDynamicArray<AIStorage>(new SortedArray<AIStorage>(Resources.LoadAll<AIStorage>(AIManager.StorageFolder), new AIStorageComparer()));
                }

                return _ais;
            }
        }

        internal static bool NameExists(string name)
        {
            var aiList = AIs;
            var count = aiList.count;
            for (int i = 0; i < count; i++)
            {
                if (aiList[i].name.Equals(name, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        internal static AIStorage GetById(string aiId)
        {
            var aiList = AIs;
            var count = aiList.count;
            for (int i = 0; i < count; i++)
            {
                if (aiList[i].aiId.Equals(aiId, StringComparison.Ordinal))
                {
                    return aiList[i];
                }
            }

            return null;
        }

        internal static void Refresh()
        {
            _ais = null;
        }

        internal static string EnsureValidName(string name, AIStorage target)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = "New AI";
            }
            else
            {
                var invalidChars = System.IO.Path.GetInvalidFileNameChars();
                var transformer = new StringBuilder(name.Length);
                for (int i = 0; i < name.Length; i++)
                {
                    var allowChar = true;
                    for (int j = 0; j < invalidChars.Length; j++)
                    {
                        var invalidChar = invalidChars[j];
                        if (invalidChar.Equals(name[i]))
                        {
                            allowChar = false;
                            break;
                        }
                    }

                    if (allowChar)
                    {
                        transformer.Append(name[i]);
                    }
                }

                name = transformer.ToString();
            }

            var nameBase = name;
            int idx = 0;
            var stored = AIs.FirstOrDefault(a => a.name.Equals(name, StringComparison.OrdinalIgnoreCase));
            while (stored != null && !object.ReferenceEquals(stored, target))
            {
                idx++;
                name = string.Concat(nameBase, " ", idx.ToString("##"));
                stored = AIs.FirstOrDefault(a => a.name.Equals(name, StringComparison.OrdinalIgnoreCase));
            }

            return name;
        }

        private class AIStorageComparer : IComparer<AIStorage>
        {
            public int Compare(AIStorage x, AIStorage y)
            {
                if (x.Equals(null))
                {
                    if (y.Equals(null))
                    {
                        return 0;
                    }

                    return -1;
                }
                else if (y.Equals(null))
                {
                    return 1;
                }

                return x.name.CompareTo(y.name);
            }
        }
    }
}