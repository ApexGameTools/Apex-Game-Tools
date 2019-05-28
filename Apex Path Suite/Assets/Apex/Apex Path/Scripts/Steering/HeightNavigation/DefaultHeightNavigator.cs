/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering.HeightNavigation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Apex.Services;
    using Apex.Utilities;
    using UnityEngine;

    /// <summary>
    /// The default height navigator implementation.
    /// </summary>
    [AddComponentMenu("Apex/Unit/Navigation/Default Height Navigator", 1026)]
    [ApexComponent("Steering")]
    public class DefaultHeightNavigator : MonoBehaviour, IHeightNavigator
    {
        [SerializeField, Tooltip("The gravity acceleration")]
        private float _gravity = -9.81f;

        [SerializeField]
        private ProviderType _providerType;

        private IUnitHeightProvider _heightProvider;
        private float _gravitationVelocity;

        /// <summary>
        /// Controls to what degree a unit will attempt to stick to the ground when moving down slopes.
        /// By default the unit will stick to the slope if its forward motion does not bring it to freefall.
        /// Increasing the value will make units more aggressive in sticking to the ground, i.e. they will convert their forward velocity more downwards.
        /// </summary>
        [MinCheck(0f, tooltip = "Controls to what degree a unit will attempt to stick to the ground when moving down slopes.")]
        public float groundStickynessFactor = 1f;

        /// <summary>
        /// The terminal velocity, i.e. maximum fall speed (m/s)
        /// </summary>
        [Tooltip("The terminal velocity, i.e. maximum fall speed (m/s)")]
        public float terminalVelocity = 50f;

        /// <summary>
        /// The gravity acceleration
        /// </summary>
        public float gravity
        {
            get { return _gravity; }

            set { _gravity = value; }
        }

        /// <summary>
        /// Gets or sets the provider type. This will automatically set <see cref="heightProvider"/> to the appropriate value.
        /// </summary>
        /// <value>
        /// The provider.
        /// </value>
        public ProviderType provider
        {
            get
            {
                return _providerType;
            }

            set
            {
                _providerType = value;
                _heightProvider = ResolveProvider();
            }
        }

        /// <summary>
        /// Gets or sets the height provider. This is automatically done if <see cref="provider"/> has a valid value.
        /// </summary>
        /// <value>
        /// The height provider.
        /// </value>
        public IUnitHeightProvider heightProvider
        {
            get { return _heightProvider; }

            set { _heightProvider = value; }
        }

        /// <summary>
        /// Gets the height output.
        /// </summary>
        /// <param name="input">The steering input.</param>
        /// <param name="effectiveMaxSpeed">The effective maximum speed of the unit, this may be higher than the desired speed.</param>
        /// <returns>
        /// The height output
        /// </returns>
        public HeightOutput GetHeightOutput(SteeringInput input, float effectiveMaxSpeed)
        {
            var finalVelocity = input.currentSpatialVelocity;

            var heightDiff = _heightProvider.GetHeightDelta(input);

            var deltaTime = input.deltaTime;
            var freefallThreshold = -effectiveMaxSpeed * deltaTime * this.groundStickynessFactor;

            var isGrounded = (heightDiff >= freefallThreshold);

            //Apply the needed vertical force to handle gravity or ascent/descent
            if (!isGrounded)
            {
                if (_gravitationVelocity > -this.terminalVelocity)
                {
                    _gravitationVelocity += _gravity * deltaTime;
                }

                if (_gravitationVelocity * deltaTime < heightDiff)
                {
                    _gravitationVelocity = heightDiff / deltaTime;
                }

                finalVelocity.y += _gravitationVelocity;
            }
            else
            {
                _gravitationVelocity = 0f;

                if (Mathf.Abs(heightDiff) > 1E-6f)
                {
                    finalVelocity.y += heightDiff / deltaTime;

                    finalVelocity = Vector3.ClampMagnitude(finalVelocity, effectiveMaxSpeed);
                }
            }

            return new HeightOutput
            {
                finalVelocity = finalVelocity,
                isGrounded = isGrounded
            };
        }

        /// <summary>
        /// Clones from the other component.
        /// </summary>
        /// <param name="other">The component to clone from.</param>
        public void CloneFrom(IHeightNavigator other)
        {
            var rhs = other as DefaultHeightNavigator;
            if (rhs == null)
            {
                return;
            }

            _gravity = rhs.gravity;
            _providerType = rhs._providerType;
            this.groundStickynessFactor = rhs.groundStickynessFactor;
            this.terminalVelocity = rhs.terminalVelocity;
        }

        private static ProviderType GetDefault(Collider c)
        {
            var activeHeightMode = GameServices.heightStrategy.heightMode;
            if (activeHeightMode == HeightSamplingMode.NoHeightSampling)
            {
                return ProviderType.ZeroHeight;
            }

            if (c is CapsuleCollider || c is SphereCollider)
            {
                return activeHeightMode == HeightSamplingMode.HeightMap ? ProviderType.HeightMapSphericalThreePoint : ProviderType.RaycastSphericalThreePoint;
            }

            if (c is BoxCollider || c is MeshCollider)
            {
                return activeHeightMode == HeightSamplingMode.HeightMap ? ProviderType.HeightMapBoxThreePoint : ProviderType.RaycastBoxThreePoint;
            }

            return ProviderType.ZeroHeight;
        }

        private void Awake()
        {
            //Make sure the rigidbody obeys the rules
            var rb = this.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = false;
            }

            if (_heightProvider == null)
            {
                _heightProvider = ResolveProvider();
            }
        }

        private IUnitHeightProvider ResolveProvider()
        {
            var c = GetComponent<Collider>();

            if (_providerType == ProviderType.None)
            {
                _providerType = GetDefault(c);
            }

            switch (_providerType)
            {
                case ProviderType.CapsuleCast:
                {
                    return new CapsuleCastProvider(c);
                }

                case ProviderType.SphereCast:
                {
                    return new SphereCastProvider(c);
                }

                case ProviderType.RaycastSphericalThreePoint:
                {
                    return new RaycastSphericalThreePointProvider(c);
                }

                case ProviderType.RaycastBoxThreePoint:
                {
                    return new RaycastBoxThreePointProvider(c);
                }

                case ProviderType.RaycastBoxFivePoint:
                {
                    return new RaycastBoxFivePointProvider(c);
                }

                case ProviderType.HeightMapSphericalThreePoint:
                {
                    return new HeightMapSphericalThreePointProvider(c);
                }

                case ProviderType.HeightMapBoxThreePoint:
                {
                    return new HeightMapBoxThreePointProvider(c);
                }

                case ProviderType.HeightMapBoxFivePoint:
                {
                    return new HeightMapBoxFivePointProvider(c);
                }

                case ProviderType.ZeroHeight:
                {
                    return new ZeroHeightProvider();
                }

                default:
                case ProviderType.Custom:
                {
                    var factory = this.As<IUnitHeightProviderFactory>();
                    if (factory == null)
                    {
                        throw new MissingComponentException("A IUnitHeightProviderFactory implementing component is required when height sampling mode is set to custom.");
                    }

                    return factory.Create(c);
                }
            }
        }
    }
}
