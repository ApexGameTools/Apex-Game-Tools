/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    using System;
    using Apex;
    using Apex.Common;
    using Apex.Debugging;
    using Apex.Messages;
    using Apex.Services;
    using UnityEngine;

    /// <summary>
    /// Component for setting up <see cref="GridPortal" />s at design time.
    /// </summary>
    [AddComponentMenu("Apex/Game World/Portals/Portal", 1019)]
    [ApexComponent("Game World")]
    public class GridPortalComponent : ExtendedMonoBehaviour, IHandleMessage<GridStatusMessage>
    {
        /// <summary>
        /// The portal name
        /// </summary>
        [Tooltip("The portal's name, which can be used to look up the portal through the GridManager.")]
        public string portalName;

        /// <summary>
        /// The type of the portal, which determines how it affects path finding.
        /// </summary>
        [Tooltip("Connector portals are mainly for connecting grids, but can also be sued inside one grid. Shortcuts are as there name suggests, short cuts and are evaluated differently.")]
        public PortalType type;

        /// <summary>
        /// The direction of the portal, i.e. one- or two-way
        /// </summary>
        [Tooltip("Two-way portals are usable from both end points, one-way portals are only usable from one end point.")]
        public PortalDirection direction;

        /// <summary>
        /// The bounds of the first portal
        /// </summary>
        public Bounds portalOne;

        /// <summary>
        /// The bounds of the second portal
        /// </summary>
        public Bounds portalTwo;

        /// <summary>
        /// Controls whether the portal end points are seen as relative to their parent transform
        /// </summary>
        [Tooltip("Controls whether the portal end points are seen as relative to their parent transform. IF relative they will move with the transform otherwise they will be static and remain where they were placed initially.")]
        public bool relativeToTransform;

        /// <summary>
        /// Whether to always draw the visualization gizmos, even when the gameobject is not selected.
        /// </summary>
        [Tooltip("Whether to always draw the visualization gizmos, even when the portal's gameobject is not selected.\nNote a Portal Visualizer can be added to the game world to control this for all portals.")]
        public bool drawGizmosAlways;

        /// <summary>
        /// The color of portal one when drawing portal gizmos.
        /// </summary>
        [Tooltip("The Gizmo color of portal one.")]
        public Color portalOneColor = new Color(0f, 150f / 255f, 211f / 255f, 100f / 255f);

        /// <summary>
        /// The color of portal one when drawing portal gizmos.
        /// </summary>
        [Tooltip("The Gizmo color of portal one.")]
        public Color portalTwoColor = new Color(0f, 211f / 255f, 150f / 255f, 100f / 255f);

        /// <summary>
        /// The color used to show the connection between portals.
        /// </summary>
        [Tooltip("The Gizmo color of the portal connection.")]
        public Color connectionColor = new Color(155f, 150f / 255f, 100f / 255f, 255f / 255f);

        [SerializeField, AttributeProperty("Exclusive To", "If set only units that have one or more of the specified attributes can use the portal.")]
        private int _exclusiveTo;
        private GridPortal _portal;
        private ActiveGrids _activeGrids;

        [Flags]
        private enum ActiveGrids : byte
        {
            None = 0,
            One = 1,
            Two = 2,
            Both = One | Two
        }

        /// <summary>
        /// Gets or sets the attribute mask that defines which units can use this portal.
        /// If set to a value other than <see cref="AttributeMask.None"/> only units with at least one of the specified attributes can use the portal.
        /// </summary>
        /// <value>
        /// The exclusive mask.
        /// </value>
        public AttributeMask exclusiveTo
        {
            get
            {
                return _exclusiveTo;
            }

            set
            {
                _exclusiveTo = value;
                if (_portal != null)
                {
                    _portal.exclusiveTo = value;
                }
            }
        }

        /// <summary>
        /// Gets a reference to the actual portal. This will be null in case the portal has not been initialized.
        /// </summary>
        public GridPortal portal
        {
            get { return _portal; }
        }

        private Bounds actualPortalOne
        {
            get
            {
                return this.relativeToTransform ? new Bounds(this.transform.TransformPoint(this.portalOne.center), this.portalOne.size) : this.portalOne;
            }
        }

        private Bounds actualPortalTwo
        {
            get
            {
                return this.relativeToTransform ? new Bounds(this.transform.TransformPoint(this.portalTwo.center), this.portalTwo.size) : this.portalTwo;
            }
        }

        /// <summary>
        /// Creates a runtime Portal instance.
        /// </summary>
        /// <typeparam name="TAction">The type of the action.</typeparam>
        /// <param name="host">The host GameObject.</param>
        /// <param name="type">The portal type.</param>
        /// <param name="portalOne">The bounds of the first portal.</param>
        /// <param name="portalTwo">The bounds of the second portal.</param>
        /// <param name="action">The action.</param>
        /// <returns>The portal</returns>
        public static GridPortalComponent Create<TAction>(GameObject host, PortalType type, Bounds portalOne, Bounds portalTwo, out TAction action) where TAction : Component, IPortalAction
        {
            return Create<TAction>(host, type, PortalDirection.Twoway, portalOne, portalTwo, false, string.Empty, out action);
        }

        /// <summary>
        /// Creates a runtime Portal instance.
        /// </summary>
        /// <typeparam name="TAction">The type of the action.</typeparam>
        /// <param name="host">The host GameObject.</param>
        /// <param name="type">The portal type.</param>
        /// <param name="portalOne">The bounds of the first portal.</param>
        /// <param name="portalTwo">The bounds of the second portal.</param>
        /// <param name="relativeToTransform">Controls whether the given bounds are treated as relative to the host transform.</param>
        /// <param name="action">The action.</param>
        /// <returns>The portal</returns>
        public static GridPortalComponent Create<TAction>(GameObject host, PortalType type, Bounds portalOne, Bounds portalTwo, bool relativeToTransform, out TAction action) where TAction : Component, IPortalAction
        {
            return Create<TAction>(host, type, PortalDirection.Twoway, portalOne, portalTwo, relativeToTransform, string.Empty, out action);
        }

        /// <summary>
        /// Creates a runtime Portal instance.
        /// </summary>
        /// <typeparam name="TAction">The type of the action.</typeparam>
        /// <param name="host">The host GameObject.</param>
        /// <param name="type">The portal type.</param>
        /// <param name="portalOne">The bounds of the first portal.</param>
        /// <param name="portalTwo">The bounds of the second portal.</param>
        /// <param name="portalName">Name of the portal.</param>
        /// <param name="action">The action.</param>
        /// <returns>The portal</returns>
        public static GridPortalComponent Create<TAction>(GameObject host, PortalType type, Bounds portalOne, Bounds portalTwo, string portalName, out TAction action) where TAction : Component, IPortalAction
        {
            return Create<TAction>(host, type, PortalDirection.Twoway, portalOne, portalTwo, false, portalName, out action);
        }

        /// <summary>
        /// Creates a runtime Portal instance.
        /// </summary>
        /// <typeparam name="TAction">The type of the action.</typeparam>
        /// <param name="host">The host GameObject.</param>
        /// <param name="type">The portal type.</param>
        /// <param name="portalOne">The bounds of the first portal.</param>
        /// <param name="portalTwo">The bounds of the second portal.</param>
        /// <param name="relativeToTransform">Controls whether the given bounds are treated as relative to the host transform.</param>
        /// <param name="portalName">Name of the portal.</param>
        /// <param name="action">The action.</param>
        /// <returns>The portal</returns>
        public static GridPortalComponent Create<TAction>(GameObject host, PortalType type, Bounds portalOne, Bounds portalTwo, bool relativeToTransform, string portalName, out TAction action) where TAction : Component, IPortalAction
        {
            return Create<TAction>(host, type, PortalDirection.Twoway, portalOne, portalTwo, relativeToTransform, portalName, out action);
        }

        /// <summary>
        /// Creates a runtime Portal instance.
        /// </summary>
        /// <typeparam name="TAction">The type of the action.</typeparam>
        /// <param name="host">The host GameObject.</param>
        /// <param name="type">The portal type.</param>
        /// <param name="direction">The direction of the portal</param>
        /// <param name="portalOne">The bounds of the first portal.</param>
        /// <param name="portalTwo">The bounds of the second portal.</param>
        /// <param name="relativeToTransform">Controls whether the given bounds are treated as relative to the host transform.</param>
        /// <param name="portalName">Name of the portal.</param>
        /// <param name="action">The action.</param>
        /// <returns>The portal</returns>
        public static GridPortalComponent Create<TAction>(GameObject host, PortalType type, PortalDirection direction, Bounds portalOne, Bounds portalTwo, bool relativeToTransform, string portalName, out TAction action) where TAction : Component, IPortalAction
        {
            host.AddIfMissing<TAction>(out action);

            var p = host.AddComponent<RuntimeGridPortalComponent>();
            p.Configure(type, direction, portalOne, portalTwo, relativeToTransform, portalName);

            return p;
        }

        /// <summary>
        /// Creates a runtime Portal instance.
        /// </summary>
        /// <typeparam name="TAction">The type of the action.</typeparam>
        /// <param name="host">The host GameObject.</param>
        /// <param name="type">The portal type.</param>
        /// <param name="portalOne">The bounds of the first portal.</param>
        /// <param name="portalTwo">The bounds of the second portal.</param>
        /// <returns>The portal</returns>
        public static GridPortalComponent Create<TAction>(GameObject host, PortalType type, Bounds portalOne, Bounds portalTwo) where TAction : Component, IPortalAction
        {
            return Create<TAction>(host, type, PortalDirection.Twoway, portalOne, portalTwo, false, string.Empty);
        }

        /// <summary>
        /// Creates a runtime Portal instance.
        /// </summary>
        /// <typeparam name="TAction">The type of the action.</typeparam>
        /// <param name="host">The host GameObject.</param>
        /// <param name="type">The portal type.</param>
        /// <param name="portalOne">The bounds of the first portal.</param>
        /// <param name="portalTwo">The bounds of the second portal.</param>
        /// <param name="relativeToTransform">Controls whether the given bounds are treated as relative to the host transform.</param>
        /// <returns>The portal</returns>
        public static GridPortalComponent Create<TAction>(GameObject host, PortalType type, Bounds portalOne, Bounds portalTwo, bool relativeToTransform) where TAction : Component, IPortalAction
        {
            return Create<TAction>(host, type, PortalDirection.Twoway, portalOne, portalTwo, relativeToTransform, string.Empty);
        }

        /// <summary>
        /// Creates a runtime Portal instance.
        /// </summary>
        /// <typeparam name="TAction">The type of the action.</typeparam>
        /// <param name="host">The host GameObject.</param>
        /// <param name="type">The portal type.</param>
        /// <param name="portalOne">The bounds of the first portal.</param>
        /// <param name="portalTwo">The bounds of the second portal.</param>
        /// <param name="portalName">Name of the portal.</param>
        /// <returns>The portal</returns>
        public static GridPortalComponent Create<TAction>(GameObject host, PortalType type, Bounds portalOne, Bounds portalTwo, string portalName) where TAction : Component, IPortalAction
        {
            return Create<TAction>(host, type, PortalDirection.Twoway, portalOne, portalTwo, false, portalName);
        }

        /// <summary>
        /// Creates a runtime Portal instance.
        /// </summary>
        /// <typeparam name="TAction">The type of the action.</typeparam>
        /// <param name="host">The host GameObject.</param>
        /// <param name="type">The portal type.</param>
        /// <param name="portalOne">The bounds of the first portal.</param>
        /// <param name="portalTwo">The bounds of the second portal.</param>
        /// <param name="relativeToTransform">Controls whether the given bounds are treated as relative to the host transform.</param>
        /// <param name="portalName">Name of the portal.</param>
        /// <returns>The portal</returns>
        public static GridPortalComponent Create<TAction>(GameObject host, PortalType type, Bounds portalOne, Bounds portalTwo, bool relativeToTransform, string portalName) where TAction : Component, IPortalAction
        {
            return Create<TAction>(host, type, PortalDirection.Twoway, portalOne, portalTwo, relativeToTransform, portalName);
        }

        /// <summary>
        /// Creates a runtime Portal instance.
        /// </summary>
        /// <typeparam name="TAction">The type of the action.</typeparam>
        /// <param name="host">The host GameObject.</param>
        /// <param name="type">The portal type.</param>
        /// <param name="direction">The direction of the portal.</param>
        /// <param name="portalOne">The bounds of the first portal.</param>
        /// <param name="portalTwo">The bounds of the second portal.</param>
        /// <param name="relativeToTransform">Controls whether the given bounds are treated as relative to the host transform.</param>
        /// <param name="portalName">Name of the portal.</param>
        /// <returns>The portal</returns>
        public static GridPortalComponent Create<TAction>(GameObject host, PortalType type, PortalDirection direction, Bounds portalOne, Bounds portalTwo, bool relativeToTransform, string portalName) where TAction : Component, IPortalAction
        {
            TAction action;
            host.AddIfMissing<TAction>(out action);

            var p = host.AddComponent<RuntimeGridPortalComponent>();
            p.Configure(type, direction, portalOne, portalTwo, relativeToTransform, portalName);

            return p;
        }

        /// <summary>
        /// Creates a runtime Connector Portal instance.
        /// </summary>
        /// <param name="host">The host GameObject.</param>
        /// <param name="portalOne">The bounds of the first portal.</param>
        /// <param name="portalTwo">The bounds of the second portal.</param>
        /// <returns>The connector portal</returns>
        public static GridPortalComponent CreateConnector(GameObject host, Bounds portalOne, Bounds portalTwo)
        {
            return CreateConnector(host, PortalDirection.Twoway, portalOne, portalTwo, false);
        }

        /// <summary>
        /// Creates a runtime Connector Portal instance.
        /// </summary>
        /// <param name="host">The host GameObject.</param>
        /// <param name="portalOne">The bounds of the first portal.</param>
        /// <param name="portalTwo">The bounds of the second portal.</param>
        /// <param name="relativeToTransform">Controls whether the given bounds are treated as relative to the host transform.</param>
        /// <returns>The connector portal</returns>
        public static GridPortalComponent CreateConnector(GameObject host, Bounds portalOne, Bounds portalTwo, bool relativeToTransform)
        {
            return CreateConnector(host, PortalDirection.Twoway, portalOne, portalTwo, relativeToTransform);
        }

        /// <summary>
        /// Creates a runtime Connector Portal instance.
        /// </summary>
        /// <param name="host">The host GameObject.</param>
        /// <param name="direction">The direction of the portal.</param>
        /// <param name="portalOne">The bounds of the first portal.</param>
        /// <param name="portalTwo">The bounds of the second portal.</param>
        /// <param name="relativeToTransform">Controls whether the given bounds are treated as relative to the host transform.</param>
        /// <returns>The connector portal</returns>
        public static GridPortalComponent CreateConnector(GameObject host, PortalDirection direction, Bounds portalOne, Bounds portalTwo, bool relativeToTransform)
        {
            var p = host.AddComponent<RuntimeGridPortalComponent>();
            p.Configure(PortalType.Connector, direction, portalOne, portalTwo, relativeToTransform, string.Empty);

            return p;
        }

        internal bool Connects(GridComponent grid)
        {
            var b = grid.bounds;
            return (b.Contains(this.actualPortalOne.center) || b.Contains(this.actualPortalTwo.center));
        }

        internal bool Connects(IGrid grid)
        {
            var b = grid.bounds;
            return (b.Contains(this.actualPortalOne.center) || b.Contains(this.actualPortalTwo.center));
        }

        internal bool Connects(GridComponent gridOne, GridComponent gridTwo)
        {
            var b1 = gridOne.bounds;
            var b2 = gridTwo.bounds;
            return (b1.Contains(this.actualPortalOne.center) || b1.Contains(this.actualPortalTwo.center)) &&
                   (b2.Contains(this.actualPortalOne.center) || b2.Contains(this.actualPortalTwo.center));
        }

        internal bool Connects(IGrid gridOne, IGrid gridTwo)
        {
            var b1 = gridOne.bounds;
            var b2 = gridTwo.bounds;
            return (b1.Contains(this.actualPortalOne.center) || b1.Contains(this.actualPortalTwo.center)) &&
                   (b2.Contains(this.actualPortalOne.center) || b2.Contains(this.actualPortalTwo.center));
        }

        /// <summary>
        /// Called on Awake
        /// </summary>
        protected virtual void Awake()
        {
            if (GridManager.instance.GetGrid(this.actualPortalOne.center) != null)
            {
                _activeGrids |= ActiveGrids.One;
            }

            if (GridManager.instance.GetGrid(this.actualPortalTwo.center) != null)
            {
                _activeGrids |= ActiveGrids.Two;
            }

            if (_activeGrids == ActiveGrids.Both)
            {
                Initialize();
            }

            GridManager.instance.RegisterPortalComponent(this);
            GameServices.messageBus.Subscribe(this);
        }

        /// <summary>
        /// Called on Start and OnEnable, but only one of the two, i.e. at startup it is only called once.
        /// </summary>
        protected override void OnStartAndEnable()
        {
            if (_portal != null)
            {
                _portal.enabled = true;
            }
        }

        private void OnDisable()
        {
            if (_portal != null)
            {
                _portal.enabled = false;
            }
        }

        private void OnDestroy()
        {
            GridManager.instance.UnregisterPortalComponent(this);
            GameServices.messageBus.Unsubscribe(this);
        }

        private void OnDrawGizmos()
        {
            if (!this.enabled)
            {
                return;
            }

            if (this.drawGizmosAlways || (PortalVisualizer.instance != null && PortalVisualizer.instance.drawAllPortals))
            {
                DrawVisualization();
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (this.drawGizmosAlways || (PortalVisualizer.instance != null && PortalVisualizer.instance.drawAllPortals))
            {
                return;
            }

            DrawVisualization();
        }

        private void DrawVisualization()
        {
            var p1 = this.actualPortalOne;
            var p2 = this.actualPortalTwo;

            Gizmos.color = this.portalOneColor;
            Gizmos.DrawCube(p1.center, p1.size);
            Gizmos.color = this.portalTwoColor;
            Gizmos.DrawCube(p2.center, p2.size);

            if (!Application.isPlaying || (_portal != null && _portal.enabled))
            {
                Gizmos.color = this.connectionColor;
                Gizmos.DrawLine(p1.center, p2.center);

                if (this.direction == PortalDirection.Twoway)
                {
                    Gizmos.DrawSphere(p1.center, 0.5f);
                    Gizmos.DrawSphere(p2.center, 0.5f);
                }
                else
                {
                    Gizmos.DrawSphere(p1.center, 0.5f);
                }
            }
        }

        private void Initialize()
        {
            IPortalAction action;
            if (this.type != PortalType.Connector)
            {
                action = this.As<IPortalAction>();
                if (action == null)
                {
                    var fact = this.As<IPortalActionFactory>();
                    if (fact != null)
                    {
                        action = fact.Create();
                    }
                }

                if (action == null)
                {
                    Debug.LogError("A portal must have an accompanying portal action component, please add one.");
                    this.enabled = false;
                    return;
                }
            }
            else
            {
                action = PortalConnectorAction.instance;
            }

            if (this.portalOne.size.sqrMagnitude == 0f || this.portalTwo.size.sqrMagnitude == 0f)
            {
                Debug.LogError("A portal's end points must have a size greater than 0.");
                this.enabled = false;
                return;
            }

            try
            {
                _portal = GridPortal.Create(this.portalName, this.type, this.direction, this.actualPortalOne, this.actualPortalTwo, this.exclusiveTo, action);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                this.enabled = false;
            }
        }

        void IHandleMessage<GridStatusMessage>.Handle(GridStatusMessage message)
        {
            var gridBounds = message.gridBounds;
            var currentActive = _activeGrids;
            var delta = ActiveGrids.None;

            if (gridBounds.Contains(this.actualPortalOne.center))
            {
                delta |= ActiveGrids.One;
            }

            if (gridBounds.Contains(this.actualPortalTwo.center))
            {
                delta |= ActiveGrids.Two;
            }

            if (delta != ActiveGrids.None)
            {
                switch (message.status)
                {
                    case GridStatusMessage.StatusCode.DisableComplete:
                    {
                        _activeGrids &= ~delta;
                        break;
                    }

                    case GridStatusMessage.StatusCode.InitializationComplete:
                    {
                        _activeGrids |= delta;
                        break;
                    }
                }

                //If this resulted in no change, return. This will happen when doing run time creation as the messages are delayed, e.g. a portal already initialized will receive a message that its grid is initialized. 
                if (_activeGrids == currentActive)
                {
                    return;
                }

                if (_activeGrids == ActiveGrids.Both)
                {
                    Initialize();

                    if (_portal != null)
                    {
                        _portal.enabled = this.enabled;
                    }
                }
                else if (_portal != null)
                {
                    GridManager.instance.UnregisterPortal(_portal.name);
                    _portal.enabled = false;
                    _portal = null;
                }
            }
        }

        private class RuntimeGridPortalComponent : GridPortalComponent
        {
            internal void Configure(PortalType type, PortalDirection direction, Bounds portalOne, Bounds portalTwo, bool relativeToTransform, string portalName)
            {
                this.type = type;
                this.direction = direction;
                this.portalOne = portalOne;
                this.portalTwo = portalTwo;
                this.relativeToTransform = relativeToTransform;
                this.portalName = portalName;

                base.Awake();
                OnStartAndEnable();
            }

            protected override void Awake()
            {
                /* NOOP */
            }
        }
    }
}
