/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Serialization
{
    /// <summary>
    /// Marker attribute that marks a <see cref="System.Guid"/> property or field as a reference to an AI.
    /// </summary>
    /// <seealso cref="Apex.AI.Serialization.MemberEditorAttribute" />
    public sealed class AIReferenceAttribute : MemberEditorAttribute
    {
    }
}
