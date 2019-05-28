/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Examples.Steering
{
    using System;
    using Apex.DataStructures;
    using Apex.LoadBalancing;
    using Apex.PathFinding;
    using Apex.Services;
    using Apex.Units;
    using Apex.WorldGeometry;
    using UnityEngine;

    [AddComponentMenu("Apex/Examples/Pickup Master", 1017)]
    [RequireComponent(typeof(SphereCollider))]
    public class PickupMaster : MonoBehaviour
    {
        public GameObject itemMold;
        public GameObject workerMold;
        public RequestType requestType;

        private IUnitFacade _workerModel;
        private SimpleQueue<PickupWorker> _availableWorkers = new SimpleQueue<PickupWorker>();
        private PriorityQueueFifo<ItemEntry> _itemsForPickup = new PriorityQueueFifo<ItemEntry>(20, QueueType.Min);
        private float _radius;

        private void Awake()
        {
            _radius = this.GetComponent<SphereCollider>().radius;

            //This could be more elegantly made, but will suffice for this example
            _workerModel = FindObjectOfType<PickupWorker>().GetUnitFacade();
        }

        private void Start()
        {
            //An action to send out workers, is scheduled for periodic execution
            //This is an alternative to implementing the ILoadBalanced interface
            var action = new RepeatableAction((ignored) =>
            {
                SendWorkers();
                return true;
            });

            LoadBalancer.defaultBalancer.Add(action, 0.3f, true);
        }

        private void Update()
        {
            RaycastHit hit;

            //Simple input receiver to allow spawning of items and workers at runtime Q = new item and W = new worker
            if (Input.GetKeyUp(KeyCode.Q))
            {
                UnityServices.mainCamera.ScreenToLayerHit(Input.mousePosition, Layers.terrain, 1000.0f, out hit);
                var item = Instantiate(this.itemMold, hit.point, Quaternion.identity) as GameObject;
                item.SetActive(true);
            }
            else if (Input.GetKeyUp(KeyCode.W))
            {
                UnityServices.mainCamera.ScreenToLayerHit(Input.mousePosition, Layers.terrain, 1000.0f, out hit);
                var worker = Instantiate(this.workerMold, hit.point, Quaternion.identity) as GameObject;
                worker.SetActive(true);
            }
        }

        private void OnGUI()
        {
            GUI.Label(new Rect(10, 10, 150, 120), "Press Q to spawn a crate at the mouse position.\n\nPress W to spawn a worker at the mouse position.");
        }

        public void RegisterWorker(PickupWorker worker)
        {
            //Start by calling workers to come to the master for an assignment
            ComeToMaster(worker);
        }

        public void RegisterItem(PickupItem item)
        {
            //The result callback for use with our path request
            Action<PathResult> callback = (result) =>
            {
                //If a path was not found, that means the item is inaccessible
                if (result.status != PathingStatus.Complete)
                {
                    return;
                }

                //Once a result is received, the item is added to the priority queue, with the cost of the path as the priority.
                //This ensures that the items closest to the master will be at the font of the queue, and hence will be the first to be picked up.
                //Since this example shows two possible options on how to use a manual request, the item is stored with its path
                var entry = new ItemEntry
                {
                    item = item,
                    pathToItem = result.path
                };

                //Since the callback is marshalled onto the main thread, there is no need to synchronize access
                _itemsForPickup.Enqueue(entry, result.pathCost);
            };

            //The radius should of course be taken off the unit type that is doing the pick-up, but keeping it simple...
            var req = new CallbackPathRequest(callback)
            {
                from = this.transform.position,
                to = item.transform.position,
                type = this.requestType,
                requesterProperties = _workerModel,
                pathFinderOptions = _workerModel.pathFinderOptions
            };

            GameServices.pathService.QueueRequest(req);
        }

        public void PickupComplete(PickupWorker worker)
        {
            ComeToMaster(worker);
        }

        private void OnTriggerEnter(Collider other)
        {
            var worker = other.GetComponent<PickupWorker>();
            if (worker != null)
            {
                //Off load the payload from the worker if it has one and enter it back into the list of available workers
                var item = worker.OffLoad();
                if (item != null && !item.Equals(null))
                {
                    Destroy(item.gameObject, 0.2f);
                }

                _availableWorkers.Enqueue(worker);
            }
        }

        private void SendWorkers()
        {
            while (_availableWorkers.count > 0 && _itemsForPickup.count > 0)
            {
                var worker = _availableWorkers.Dequeue();
                var entry = _itemsForPickup.Dequeue();

                worker.SetTarget(entry.item);

                //For Normal requests, the path is already resolved and can be used directly
                //For IntelOnly request, the path request is issued again, since it was never produced by the first request
                //Which is the best approach depends on what goes on in the world, i.e. is the path expected to still be valid or not.
                //In this particular example the Normal request would be the way to go, since nothing in the scene changes.
                if (this.requestType == RequestType.Normal)
                {
                    //Since in this case the oath was issued as being from the master's position, the first node on the path is not desired
                    entry.pathToItem.Pop();
                    worker.unit.MoveAlong(entry.pathToItem);
                }
                else
                {
                    worker.unit.MoveTo(entry.item.transform.position, false);
                }
            }
        }

        private void ComeToMaster(PickupWorker worker)
        {
            var pos = this.transform.position;
            pos = pos + Vector3.ClampMagnitude(worker.unit.position - pos, _radius * 0.5f);

            worker.unit.MoveTo(pos, false);
        }

        private class ItemEntry
        {
            public PickupItem item;
            public Path pathToItem;
        }
    }
}
