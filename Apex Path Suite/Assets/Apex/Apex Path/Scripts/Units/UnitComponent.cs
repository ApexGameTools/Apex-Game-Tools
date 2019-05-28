/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Units
{
    using Apex.Common;
    using Apex.Services;
    using Apex.Steering;
    using Apex.Utilities;
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// Basic unit properties component.
    /// </summary>
    [AddComponentMenu("Apex/Unit/Navigation/Unit", 1032)]
    [ApexComponent("Unit Properties")]
    public partial class UnitComponent : AttributedComponent, IUnitProperties
    {
        private Transform _transform;
        private bool _isSelected;
        private bool? _selectPending;
        private HeightNavigationCapabilities _effectiveHeightCapabilities;

        [SerializeField, Tooltip("The determination factor is used to evaluate whether this unit separates or avoids other units. Units with lower determination are basically ignored.")]
        private int _determination = 1;

        [SerializeField, Tooltip("Controls whether this unit is selectable via user input such as a mouse.")]
        private bool _isSelectable = false;

        [SerializeField]
        private HeightNavigationCapabilities _heightCapabilities = new HeightNavigationCapabilities
        {
            maxClimbHeight = 0.5f,
            maxDropHeight = 1f,
            maxSlopeAngle = 30f
        };

        /// <summary>
        /// The visual used to indicate whether the unit is selected or not.
        /// </summary>
        [Tooltip("The visual used to indicate whether the unit is selected or not.")]
        public GameObject selectionVisual;

        /// <summary>
        /// The radius of the unit.
        /// </summary>
        [MinCheck(0f, label = "Radius", tooltip = "The radius of the unit.")]
        public float radius = 0.5f;

        /// <summary>
        /// The field of view in degrees
        /// </summary>
        [RangeX(0f, 360f, label = "Field of View", tooltip = "The field of view in degrees.")]
        public float fieldOfView = 200f;

        /// <summary>
        /// If the unit is not properly grounded at y = 0, set this offset such that when the unit is in a grounded position, its transform.y - yAxisoffset == 0.
        /// This is only relevant if your unit has no rigidbody with gravity.
        /// </summary>
        [MinCheck(0f, label = "Ground Offset", tooltip = "The distance the unit will hover above the ground as seen from the bottom of its collider/mesh.")]
        public float yAxisoffset = 0.0f;

        /// <summary>
        /// Gets the position of the component.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        public Vector3 position
        {
            get { return _transform.position; }
        }

        /// <summary>
        /// Gets the forward vector of the unit, i.e. the direction its nose is pointing (provided it has a nose).
        /// </summary>
        /// <value>
        /// The forward vector.
        /// </value>
        public Vector3 forward
        {
            get { return _transform.forward; }
        }

        float IUnitProperties.radius
        {
            get { return this.radius; }
        }

        float IUnitProperties.fieldOfView
        {
            get { return this.fieldOfView; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is selectable.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is selectable; otherwise, <c>false</c>.
        /// </value>
        public bool isSelectable
        {
            get { return _isSelectable; }
            set { _isSelectable = value; }
        }

        /// <summary>
        /// Gets or sets the determination factor used to evaluate whether this unit separates or avoids other units. The higher the determination, the less avoidance/separation.
        /// </summary>
        /// <value>
        /// The determination.
        /// </value>
        public int determination
        {
            get { return _determination; }
            set { _determination = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this unit is selected. Only has an impact if <see cref="isSelectable" /> is true.
        /// </summary>
        /// <value>
        ///   <c>true</c> if selected; otherwise, <c>false</c>.
        /// </value>
        public bool isSelected
        {
            get
            {
                return _isSelected;
            }

            set
            {
                _selectPending = null;

                if (_isSelected != value)
                {
                    _isSelected = value;
                    if (this.selectionVisual != null)
                    {
                        this.selectionVisual.SetActive(value);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the ground offset, i..e the distance from the bottom of the unit's collider to the ground.
        /// </summary>
        /// <value>
        /// The ground offset.
        /// </value>
        public float groundOffset
        {
            get { return this.yAxisoffset; }
        }

        /// <summary>
        /// Gets the offset between the unit's lower most point where it will touch the ground (touchdownPosition) and its position, typically the bottom of its collider to its position (y delta).
        /// </summary>
        /// <value>
        /// The position to ground offset.
        /// </value>
        public float baseToPositionOffset
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the height of the unit, i.e. from where it touches the ground to the top of its head (if it has one).
        /// </summary>
        /// <value>
        /// The height of the unit.
        /// </value>
        public float height
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the position where the unit touches the ground (if it is grounded). This is its position offset by baseToPositionOffset
        /// </summary>
        /// <value>
        /// The touchdown position.
        /// </value>
        public Vector3 basePosition
        {
            get
            {
                var tmp = _transform.position;
                tmp.y -= this.baseToPositionOffset;
                return tmp;
            }
        }

        /// <summary>
        /// Gets the height navigation capability of the unit, i.e. how steep it can climb etc.
        /// </summary>
        /// <value>
        /// The height navigation capability.
        /// </value>
        public HeightNavigationCapabilities heightNavigationCapability
        {
            get { return _effectiveHeightCapabilities; }
        }

        /// <summary>
        /// Recalculates the base position and unit height. Call this if the unit's size changes at runtime
        /// </summary>
        public void RecalculateBasePosition()
        {
            var rb = this.GetComponent<Rigidbody>();
            var coll = this.GetComponent<Collider>();
            var renderer = this.GetComponent<Renderer>();

            if (rb != null && rb.useGravity)
            {
                yAxisoffset = 0.0f;
            }

            float totalOffset;
            if (coll != null)
            {
                //We need to adjust the collider to take the collider overlap into consideration
                var m = new ColliderMeasurement(coll, true);
                totalOffset = m.extents.y - m.transformCenterOffset.y + this.yAxisoffset;
                this.height = m.size.y + this.yAxisoffset;

                if (rb != null && rb.useGravity)
                {
#if UNITY_5 || UNITY_2017
                    totalOffset -= (2 * coll.contactOffset);
#else
                    totalOffset -= (2 * Physics.minPenetrationForPenalty);
#endif
                }
            }
            else if (renderer != null)
            {
                var posOffset = (this.position.y - renderer.bounds.center.y);
                totalOffset = renderer.bounds.extents.y + posOffset + this.yAxisoffset;
                this.height = renderer.bounds.size.y + this.yAxisoffset;
            }
            else
            {
                totalOffset = this.yAxisoffset;
            }

            this.baseToPositionOffset = totalOffset;
        }

        /// <summary>
        /// Marks the unit as pending for selection. This is used to indicate a selection is progress, before the actual selection occurs.
        /// </summary>
        /// <param name="pending">if set to <c>true</c> the unit is pending for selection otherwise it is not.</param>
        public void MarkSelectPending(bool pending)
        {
            if (pending != _selectPending)
            {
                _selectPending = pending;

                if (this.selectionVisual != null)
                {
                    this.selectionVisual.SetActive(pending);
                }
            }
        }

        private void Awake()
        {
            this.WarnIfMultipleInstances<IUnitProperties>();

            _transform = this.transform;

            var heightStrat = GameServices.heightStrategy;
            _effectiveHeightCapabilities = heightStrat.useGlobalHeightNavigationSettings ? heightStrat.unitsHeightNavigationCapability : _heightCapabilities;
            GameServices.gameStateManager.RegisterUnit(this.gameObject);
        }

        /// <summary>
        /// Called when enabled.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            //Ensure selection is toggled off
            _isSelected = true;
            this.isSelected = false;

            //Get base pos
            RecalculateBasePosition();

            //Make sure units do not start embedded in the ground
            RaycastHit groundHit;
            var basePos = this.basePosition;
            var topPos = basePos;
            topPos.y += this.height;
            if (Physics.Raycast(topPos, Vector3.down, out groundHit, float.PositiveInfinity, Layers.terrain))
            {
                var diff = groundHit.point.y - basePos.y;
                if (diff > 0f)
                {
                    var pos = _transform.position;
                    pos.y += diff;
                    _transform.position = pos;
                }
            }
        }
        
        private void OnDestroy()
        {
            GameServices.gameStateManager.UnregisterUnit(this.gameObject);
        }

        /// <summary>
        /// Clones from the other component.
        /// </summary>
        /// <param name="unitComp">The component to clone from.</param>
        public void CloneFrom(UnitComponent unitComp)
        {
            _isSelectable = unitComp.isSelectable;
            this.selectionVisual = unitComp.selectionVisual;
            this.attributes = unitComp.attributes;
            _heightCapabilities = unitComp._heightCapabilities;
            _effectiveHeightCapabilities = unitComp._effectiveHeightCapabilities;

            this.radius = unitComp.radius;
            this.fieldOfView = unitComp.fieldOfView;
            this.baseToPositionOffset = unitComp.baseToPositionOffset;
            
            this.height = unitComp.height;
            this.yAxisoffset = unitComp.yAxisoffset;

            this.determination = unitComp.determination;
        }
    }
}
