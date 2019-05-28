namespace Apex.AI.Editor
{
    using System;
    using System.Text.RegularExpressions;

    internal class TypeNameTokens
    {
        internal TypeNameTokens(string completeTypeName)
        {
            //Since the format is well know this performs far better than a regex
            var outer = completeTypeName.Split(',');
            this.completeTypeName = string.Concat(outer[0], ",", outer[1]);
            this.fullTypeName = outer[0];
            this.assemblyName = outer[1].Trim();

            var inner = outer[0].Split('.');
            this.simpleTypeName = inner[inner.Length - 1];
        }

        internal string completeTypeName { get; private set; }

        internal string simpleTypeName { get; private set; }

        internal string fullTypeName { get; private set; }

        internal string assemblyName { get; private set; }
    }
}
