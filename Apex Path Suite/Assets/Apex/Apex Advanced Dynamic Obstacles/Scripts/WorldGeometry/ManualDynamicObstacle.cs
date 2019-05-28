/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.WorldGeometry
{
    /// <summary>
    /// A dynamic obstacle that can be instantiated on the fly. It is activated / deactivated manually.
    /// </summary>
    public partial class ManualDynamicObstacle
    {
        void IDynamicObstacle.RefreshHull()
        {
            /* NOOP */
        }
    }
}
