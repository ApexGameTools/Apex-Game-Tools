namespace Apex.Examples.Misc
{
    using Apex.Steering.Components;
    using Apex.Units;
    using UnityEngine;

    /// <summary>
    /// An example behavior that makes units wait for an <see cref="ObjectPendler"/>
    /// </summary>
    [AddComponentMenu("Apex/Examples/Wait for Object Pendler", 1026)]
    public class WaitForPendle : MonoBehaviour
    {
        /// <summary>
        /// The pendler
        /// </summary>
        public ObjectPendler pendler;

        /// <summary>
        /// The position this moves the pendler to
        /// </summary>
        public ObjectPendler.Position movesTo;

        /// <summary>
        /// Whether this is an on-board (the pendler) trigger
        /// </summary>
        public bool isOnboardTrigger;

        private bool _moveOnExit;

        private void OnTriggerEnter(Collider other)
        {
            if (pendler == null)
            {
                return;
            }

            if (pendler.IsAtPosition(this.movesTo))
            {
                _moveOnExit = false;
                return;
            }

            var steerer = other.GetUnitFacade();
            if (steerer == null)
            {
                return;
            }

            if (this.isOnboardTrigger)
            {
                _moveOnExit = true;
                steerer.DisableMovementOrders();
            }
            else
            {
                StartPendle(steerer);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (this.isOnboardTrigger)
            {
                var steerer = other.GetUnitFacade();
                if (steerer == null)
                {
                    return;
                }

                if (_moveOnExit)
                {
                    StartPendle(steerer);
                }
                else
                {
                    steerer.EnableMovementOrders();
                }
            }
        }

        private void StartPendle(IUnitFacade steerer)
        {
            steerer.DisableMovementOrders();
            steerer.Wait(null);

            var curParent = steerer.transform.parent;
            var rb = steerer.gameObject.GetComponent<Rigidbody>();

            if (this.isOnboardTrigger)
            {
                rb.isKinematic = true;
                steerer.transform.parent = pendler.transform;
            }

            pendler.MoveTo(
                this.movesTo,
                () =>
                {
                    if (this.isOnboardTrigger)
                    {
                        steerer.transform.parent = curParent;
                        rb.isKinematic = false;
                    }
                    else
                    {
                        steerer.EnableMovementOrders();
                    }

                    steerer.Resume();
                });
        }
    }
}
