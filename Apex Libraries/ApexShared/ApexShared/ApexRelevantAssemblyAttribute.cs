/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex
{
    using System;

    /// <summary>
    /// Marks an assembly as containing implementations relevant to the function of Apex Products.
    /// Only assemblies marked with this attribute will be searched for relevant classes etc.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class ApexRelevantAssemblyAttribute : Attribute
    {
    }
}
