/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Editor.UndoRedo
{
    internal abstract class AIUIOperation
    {
        protected AIUI _ui;

        internal AIUIOperation(AIUI ui)
        {
            _ui = ui;
        }
    }
}
