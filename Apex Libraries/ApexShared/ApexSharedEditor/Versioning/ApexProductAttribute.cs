/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Editor.Versioning
{
    using System;

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class ApexProductAttribute : Attribute, IProductIdentifier
    {
        public ApexProductAttribute(string name, string version, ProductType type)
        {
            this.name = name;
            this.generalName = name;
            this.version = version;
            this.type = type;
        }

        public ApexProductAttribute(string generalName, string variant, string version, ProductType type)
        {
            this.name = string.Concat(generalName, ' ', variant);
            this.generalName = generalName;
            this.version = version;
            this.type = type;
        }

        public string name
        {
            get;
            private set;
        }

        public string generalName
        {
            get;
            private set;
        }

        public string version
        {
            get;
            private set;
        }

        public ProductType type
        {
            get;
            private set;
        }

        public bool requiresLicense
        {
            get;
            set;
        }
    }
}
