/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Examples.SceneSpecific.RuntimeGrids
{
    using Apex.Messages;
    using Apex.Services;
    using UnityEngine;

    public class UnitSpawner : MonoBehaviour, IHandleMessage<GridStatusMessage>
    {
        public GameObject unitMold;
        public Vector3 spawnPoint;

        private void OnEnable()
        {
            GameServices.messageBus.Subscribe(this);
        }

        private void OnDisable()
        {
            GameServices.messageBus.Unsubscribe(this);
        }

        public void Handle(GridStatusMessage message)
        {
            if (!message.gridBounds.Contains(this.spawnPoint))
            {
                return;
            }

            switch (message.status)
            {
                case GridStatusMessage.StatusCode.InitializationComplete:
                {
                    SpawnUnit();
                    break;
                }

                case GridStatusMessage.StatusCode.DisableComplete:
                {
                    //Do something in response to the grid being disabled
                    break;
                }
            }
        }

        private void SpawnUnit()
        {
            var go = Instantiate(this.unitMold, this.spawnPoint, Quaternion.identity) as GameObject;
            go.SetActive(true);

            var unit = go.GetUnitFacade();

            unit.MoveTo(new Vector3(10f, 0f, 10f), false);
        }
    }
}
