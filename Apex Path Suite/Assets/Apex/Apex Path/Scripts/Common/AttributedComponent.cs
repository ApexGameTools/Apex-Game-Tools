/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Common
{
    using UnityEngine;

    /// <summary>
    /// Possible base class for components that want to use the Attributes feature.
    /// </summary>
    public abstract class AttributedComponent : ExtendedMonoBehaviour, IHaveAttributes
    {
        [SerializeField, AttributeProperty("Attributes", "The custom attributes of this entity.")]
        private int _attributeMask;

        /// <summary>
        /// Gets or sets the attributes.
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        public AttributeMask attributes
        {
            get
            {
                return _attributeMask;
            }

            set
            {
                if (_attributeMask != value)
                {
                    var currrent = _attributeMask;
                    _attributeMask = value;

                    OnAttributesChanged(currrent);
                }
            }
        }

        /// <summary>
        /// Called when attributes changed.
        /// </summary>
        /// <param name="previous">The previous attributes, before the change.</param>
        protected virtual void OnAttributesChanged(AttributeMask previous)
        {
        }
    }
}
