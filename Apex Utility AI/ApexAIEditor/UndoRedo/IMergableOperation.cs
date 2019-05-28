/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Editor.UndoRedo
{
    internal interface IMergableOperation
    {
        bool TryMergeWith(IUndoRedo other, bool isBulkOperation);
    }
}
