#pragma warning disable 1591
namespace Apex.Examples.Extensibility
{
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// Example of using a factory to define a portal action.
    /// </summary>
    [AddComponentMenu("Apex/Examples/Extensibility/Portal Action Random", 1008)]
    public class PortalActionRandomComponent : MonoBehaviour, IPortalActionFactory
    {
        public IPortalAction Create()
        {
            //Please note that while this example uses MonoBehaviour implementations of the Portal Actions this is not a requirement.
            //In fact that main idea of having a factory like this, is to enable portal actions that are not MonoBehaviours.
            var roll = Random.Range(0, 9);
            if (roll < 5)
            {
                return this.gameObject.AddComponent<PortalActionJumpComponent>();
            }

            return this.gameObject.AddComponent<PortalActionTeleportComponent>();
        }
    }
}
