/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Examples.Steering
{
    using Apex.Steering;
    using Apex.Steering.Components;
    using Apex.Units;
    using UnityEngine;

    [AddComponentMenu("Apex/Examples/Steer to Follow", 1025)]
    public class SteerToFollowComponent : ArrivalBase
    {
        public UnitComponent unitToFollow;

        public float separation = 1.0f;

        public override void GetDesiredSteering(SteeringInput input, SteeringOutput output)
        {
            var separation = this.unitToFollow.radius + input.unit.radius + this.separation;

            var targetPos = this.unitToFollow.position;
            var remainingDistance = (input.unit.position - targetPos).magnitude - separation;

            Arrive(this.unitToFollow.position, remainingDistance, input, output);
        }
    }
}
