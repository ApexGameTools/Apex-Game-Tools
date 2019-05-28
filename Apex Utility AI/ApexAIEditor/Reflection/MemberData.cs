/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor.Reflection
{
    using System;
    using System.Reflection;

    public sealed class MemberData
    {
        public MemberInfo member;
        public Type rendererType;
        public string name;
        public string description;
        internal string category;
        internal int outerSortOrder;
        internal int innerSortOrder;
    }
}