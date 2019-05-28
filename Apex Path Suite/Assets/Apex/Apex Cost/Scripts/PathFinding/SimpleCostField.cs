/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.PathFinding
{
    using Apex.Messages;
    using Apex.Services;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// A simple cost field that applies a static cost to a number of cells
    /// </summary>
    [AddComponentMenu("Apex/Game World/Simple Cost Field", 1042)]
    public class SimpleCostField : ExtendedMonoBehaviour, IHandleMessage<GridStatusMessage>
    {
        /// <summary>
        /// The cost applied
        /// </summary>
        public int cost;

        /// <summary>
        /// The bounds to cover
        /// </summary>
        public Bounds bounds;

        /// <summary>
        /// Whther bounds are seen relative to the transform
        /// </summary>
        public bool relativeToTransform;

        /// <summary>
        /// The color of the field for drawing gizmos.
        /// </summary>
        public Color gizmoColor = new Color(210f / 255f, 205f / 255f, 0f, 100f / 255f);

        private Bounds actualBounds
        {
            get
            {
                return this.relativeToTransform ? new Bounds(this.transform.TransformPoint(this.bounds.center), this.bounds.size) : this.bounds;
            }
        }

        private void Awake()
        {
            GameServices.messageBus.Subscribe(this);
        }

        /// <summary>
        /// Called on Start and OnEnable, but only one of the two, i.e. at startup it is only called once.
        /// </summary>
        protected override void OnStartAndEnable()
        {
            var b = this.actualBounds;

            var grid = GridManager.instance.GetGrid(b.center);
            if (grid == null)
            {
                return;
            }

            foreach (var c in grid.GetCoveredCells(b))
            {
                c.cost += this.cost;
            }
        }

        private void OnDisable()
        {
            var b = this.actualBounds;

            var grid = GridManager.instance.GetGrid(b.center);
            if (grid == null)
            {
                return;
            }

            foreach (var c in grid.GetCoveredCells(b))
            {
                c.cost -= this.cost;
            }
        }

        private void OnDestroy()
        {
            GameServices.messageBus.Unsubscribe(this);
        }

        private void OnDrawGizmosSelected()
        {
            if (!this.enabled)
            {
                return;
            }

            var b = this.actualBounds;

            Gizmos.color = this.gizmoColor;
            Gizmos.DrawCube(b.center, b.size);
        }

        void IHandleMessage<GridStatusMessage>.Handle(GridStatusMessage message)
        {
            var gridBounds = message.gridBounds;
            if (!gridBounds.Contains(this.actualBounds.center))
            {
                return;
            }

            switch (message.status)
            {
                case GridStatusMessage.StatusCode.DisableComplete:
                {
                    this.enabled = false;
                    break;
                }

                case GridStatusMessage.StatusCode.InitializationComplete:
                {
                    this.enabled = true;
                    break;
                }
            }
        }
    }
}
