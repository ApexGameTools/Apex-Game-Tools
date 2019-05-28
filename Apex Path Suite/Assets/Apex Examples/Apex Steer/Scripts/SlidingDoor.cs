#pragma warning disable 1591
namespace Apex.Examples.Steering
{
    using Apex.Examples.Misc;
    using Apex.LoadBalancing;
    using Apex.Steering.Components;
    using Apex.Units;
    using UnityEngine;

    /// <summary>
    /// An example behavior that makes units wait for an <see cref="ObjectPendler"/>
    /// </summary>
    [AddComponentMenu("Apex/Examples/Sliding Door", 1022)]
    public class SlidingDoor : MonoBehaviour
    {
        private ObjectPendler _pendler;

        private void Start()
        {
            _pendler = this.GetComponent<ObjectPendler>();

            MoveForth();
        }

        private void MoveForth()
        {
            _pendler.MoveTo(ObjectPendler.Position.To, () =>
                {
                    LoadBalancer.defaultBalancer.Add(new OneTimeAction((ignored) => MoveBack()), 5.0f, true);
                });
        }

        private void MoveBack()
        {
            _pendler.MoveTo(ObjectPendler.Position.From, () =>
            {
                LoadBalancer.defaultBalancer.Add(new OneTimeAction((ignored) => MoveForth()), 5.0f, true);
            });
        }
    }
}
