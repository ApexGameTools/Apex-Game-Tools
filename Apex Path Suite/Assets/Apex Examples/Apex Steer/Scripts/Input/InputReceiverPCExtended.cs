/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Examples.Input
{
    using Apex.Examples.Extensibility;
    using Apex.Services;
    using UnityEngine;

    public partial class InputReceiverPC
    {
        partial void Steer()
        {
            if (Input.GetKeyUp(KeyCode.J))
            {
                var selected = GameServices.gameStateManager.unitSelection.selected;
                foreach (var unit in selected.All())
                {
                    var unitEx = unit.GetUnitFacade<UnitFacadeExtended>();
                    unitEx.Jump();
                }
            }
        }
    }
}
