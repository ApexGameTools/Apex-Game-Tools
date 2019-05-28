#pragma warning disable 1591
namespace Apex.Examples.SceneSpecific.UnitFacades
{
    using UnityEngine;

    [AddComponentMenu("Apex/Examples/SceneSpecific/UnitFacades/Unit Facade Example", 1013)]
    public class UnitFacadeExample : MonoBehaviour
    {
        public Transform target;

        private void Start()
        {
            var unit = this.GetUnitFacade();

            unit.MoveTo(this.target.position, false);
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.R))
            {
                var unit = this.GetUnitFacade<ExtendedUnitFacade>();

                unit.Run();
            }
        }
    }
}
