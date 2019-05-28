/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Editor.Reflection
{
    internal interface IKeyedItem
    {
        object key { get; }

        bool isDuplicate { get; }
    }
}
