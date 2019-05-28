/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.PathFinding
{
    using Apex.WorldGeometry;
    using UnityEngine;

    /// <summary>
    /// Call back delegate when units need to replan their path.
    /// </summary>
    /// <param name="unit">The unit.</param>
    /// <param name="currentDestination">The current destination, i.e. the point the unit was moving towards when the replan was issued.</param>
    /// <param name="path">The remainder of the path.</param>
    /// <returns>The updated path</returns>
    public delegate Path ReplanCallback(GameObject unit, IPositioned currentDestination, Path path);
}
