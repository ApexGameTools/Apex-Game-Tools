/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Steering.VectorFields
{
    using Apex.PathFinding;
    using Apex.Services;
    using Apex.Units;
    using UnityEngine;

    /// <summary>
    /// An <see cref="ExtendedMonoBehaviour"/> component responsible for creating <see cref="IVectorField"/>s and implicitely exposing the vector field options.
    /// </summary>
    [AddComponentMenu("Apex/Game World/Vector Field Manager", 1044)]
    [ApexComponent("Steering")]
    public class VectorFieldManagerComponent : ExtendedMonoBehaviour
    {
        /// <summary>
        /// The vector field options
        /// </summary>
        public VectorFieldOptions vectorFieldOptions;

        private void Awake()
        {
            // register itself as the vector field manager
            GameServices.vectorFieldManager = this;
        }

        /// <summary>
        /// Creates a vector field using the VectorFieldOptions exposed on this component.
        /// </summary>
        /// <param name="group">The transient unit group.</param>
        /// <param name="path">The path.</param>
        /// <returns>A new vector field, or null on error</returns>
        public IVectorField CreateVectorField(TransientGroup<IUnitFacade> group, Path path)
        {
            IVectorField newField = null;
            switch (vectorFieldOptions.vectorFieldType)
            {
                case VectorFieldType.FullGridField:
                {
                    newField = new FullGridVectorField(group, path, vectorFieldOptions);
                    break;
                }

                case VectorFieldType.ProgressiveField:
                {
                    newField = new ProgressiveVectorField(group, path, vectorFieldOptions);
                    break;
                }

                case VectorFieldType.CrossGridField:
                {
                    newField = new CrossGridVectorField(group, path, vectorFieldOptions);
                    break;
                }

                default:
                case VectorFieldType.FunnelField:
                {
                    newField = new FunnelVectorField(group, path, vectorFieldOptions);
                    break;
                }
            }

            if (newField == null)
            {
                return null;
            }

            newField.Initialize();

            return newField;
        }
    }
}