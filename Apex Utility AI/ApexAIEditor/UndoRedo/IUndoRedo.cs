/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Editor.UndoRedo
{
    internal interface IUndoRedo
    {
        void Do();

        void Undo();
    }
}
