/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Examples.Extensibility
{
    using Apex.Steering.Components;
    using Apex.Units;
    using UnityEngine;

    /// <summary>
    /// A unit facade extension. This one allows jumping.
    /// </summary>
    public class UnitFacadeExtended : UnitFacade
    {
        private SteerToJumpComponent _jumper;

        public override void Initialize(GameObject unitObject)
        {
            base.Initialize(unitObject);

            _jumper = unitObject.GetComponent<SteerToJumpComponent>();
            if (_jumper == null)
            {
                throw new MissingComponentException("A Steer To Jump Component was expected on the unit " + unitObject.name);
            }
        }

        public void Jump()
        {
            _jumper.Jump();
        }

        public void Jump(float howHigh, float force)
        {
            _jumper.Jump(howHigh, force);
        }
    }
}
