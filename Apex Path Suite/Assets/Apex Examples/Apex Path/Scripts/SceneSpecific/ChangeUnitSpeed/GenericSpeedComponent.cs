#pragma warning disable 1591
namespace Apex.Examples.SceneSpecific.ChangeUnitSpeed
{
    using Apex.Steering;
    using UnityEngine;

    [AddComponentMenu("Apex/Examples/SceneSpecific/ChangeUnitSpeed/Generic Speed", 1000)]
    public class GenericSpeedComponent : SpeedComponent
    {
        public float defaultSpeed = 5f;

        private void Awake()
        {
            _preferredSpeed = this.defaultSpeed;
        }

        public override void CloneFrom(IDefineSpeed speedComponent)
        {
            base.CloneFrom(speedComponent);

            var gs = speedComponent as GenericSpeedComponent;
            if (gs != null)
            {
                this.defaultSpeed = gs.defaultSpeed;
            }
        }
    }
}
