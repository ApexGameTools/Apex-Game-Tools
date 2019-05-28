/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Services
{
    using Apex.Steering;
    using Apex.Steering.VectorFields;
    using UnityEngine;

    /// <summary>
    /// Partial GameServices class, specific to Apex Steer.
    /// Responsible for supporting the VectorFieldManager and the NavigationSettings, which are specific to Apex Steer.
    /// </summary>
    public partial class GameServices
    {
        private static VectorFieldManagerComponent _vectorFieldManager;
        private static NavigationSettings _navigationSettings;

        /// <summary>
        /// Gets or sets the vector field manager.
        /// </summary>
        /// <exception cref="UnityEngine.MissingComponentException">No VectorFieldManager Service has been initialized, please ensure that you have a VectorFieldManager Service component in the game world.\nThis may also be caused by script files having been recompiled while the scene is running, if so restart the scene.</exception>
        public static VectorFieldManagerComponent vectorFieldManager
        {
            get
            {
                if (_vectorFieldManager == null)
                {
                    throw new MissingComponentException("No VectorFieldManager Service has been initialized, please ensure that you have a VectorFieldManager Service component in the game world.\nThis may also be caused by script files having been recompiled while the scene is running, if so restart the scene.");
                }

                return _vectorFieldManager;
            }

            set
            {
                _vectorFieldManager = value;
            }
        }

        /// <summary>
        /// Gets or sets the navigation settings.
        /// </summary>
        /// <exception cref="UnityEngine.MissingComponentException">No NavigationSettings Service has been initialized, please ensure that you have a NavigationSettings Service component in the game world.\nThis may also be caused by script files having been recompiled while the scene is running, if so restart the scene.</exception>
        public static NavigationSettings navigationSettings
        {
            get
            {
                if (_navigationSettings == null)
                {
                    throw new MissingComponentException("No NavigationSettings Service has been initialized, please ensure that you have a NavigationSettings Service component in the game world.\nThis may also be caused by script files having been recompiled while the scene is running, if so restart the scene.");
                }

                return _navigationSettings;
            }

            set
            {
                _navigationSettings = value;
            }
        }
    }
}