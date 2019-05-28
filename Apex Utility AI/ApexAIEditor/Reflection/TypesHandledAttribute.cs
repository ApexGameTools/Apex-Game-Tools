/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor.Reflection
{
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class TypesHandledAttribute : Attribute
    {
        public TypesHandledAttribute(params Type[] typesHandled)
        {
            this.typesHandled = typesHandled;
        }

        public Type[] typesHandled
        {
            get;
            set;
        }
    }
}