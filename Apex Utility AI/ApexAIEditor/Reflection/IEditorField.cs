/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.AI.Editor.Reflection
{
    public interface IEditorField
    {
        string memberName { get; }

        object currentValue { get; }

        void RenderField(AIInspectorState state);
    }
}