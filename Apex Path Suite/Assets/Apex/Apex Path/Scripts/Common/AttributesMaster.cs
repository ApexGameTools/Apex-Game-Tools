/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Common
{
    using System;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Utility class for handling attributes.
    /// </summary>
    public static class AttributesMaster
    {
        static AttributesMaster()
        {
            Refresh();
        }

        public static bool attributesEnabled
        {
            get;
            private set;
        }

        public static Type attributesEnumType
        {
            get;
            private set;
        }

        public static int highestOrderBit
        {
            get;
            private set;
        }

        public static void Refresh()
        {
            var markerAttribute = typeof(EntityAttributesEnumAttribute);

#if NETFX_CORE
            var markerAttributeInf = markerAttribute.GetTypeInfo();
            var defaultAttributeInf = typeof(DefaultEntityAttributesEnum).GetTypeInfo();
            var asm = markerAttributeInf.Assembly;

            attributesEnumType = asm.DefinedTypes.Where(t => t.IsEnum && t.CustomAttributes.Any(a => a.AttributeType == markerAttribute) && t != defaultAttributeInf).FirstOrDefault().AsType();
#else
            var asm = markerAttribute.Assembly;
            attributesEnumType = asm.GetTypes().Where(t => t.IsEnum && Attribute.IsDefined(t, markerAttribute) && t != typeof(DefaultEntityAttributesEnum)).FirstOrDefault();
#endif
            if (attributesEnumType == null)
            {
                attributesEnabled = false;
                highestOrderBit = 0;
                attributesEnumType = typeof(DefaultEntityAttributesEnum);
            }
            else
            {
                var vals = Enum.GetValues(attributesEnumType);
                highestOrderBit = (int)Math.Log((int)vals.GetValue(vals.Length - 1), 2);
                attributesEnabled = true;
            }
        }
    }
}
